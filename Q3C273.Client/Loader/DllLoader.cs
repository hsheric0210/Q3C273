using Q3C273.Shared;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

        public unsafe (IntPtr localAddress, IntPtr remoteAddress) Inject(IntPtr processHandle)
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
                throw new NativeMemoryException("Remote injected PE memory");

            // Copy headers (they're always consistent and only contains 'relative' addresses thus we don't need to touch them)
            Marshal.Copy(imageBytes, 0, localMem, (int)pe.SizeOfHeader);

            pe.CopySectionsTo(localMem);

            pe.UpdateBaseRelocations(localMem, remoteMem, pe.OriginalImageBase);

            pe.ImportDllImports(localMem, processHandle);

            if (!specifics.HasFlag(ReflectiveSpecific.NOT_NX_COMPAT))
                pe.UpdateSectionProtections(localMem);

            var written = UIntPtr.Zero;
            var state = WriteProcessMemory(processHandle, remoteMem, localMem, (UIntPtr)pe.SizeOfImage, ref written);
            if (!state || (UIntPtr)pe.SizeOfImage != written)
                throw new NativeMemoryException("Remote injected PE memory", remoteMem, (UIntPtr)pe.SizeOfImage, written);

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

            var nativeStream = new UnmanagedMemoryStream((byte*)shellCodeMem.ToPointer(), shellCodeSize, shellCodeSize, FileAccess.ReadWrite);
            nativeStream.WriteBytes(shellCode[0]);
            nativeStream.WriteObject(remoteMem);
            nativeStream.WriteBytes(shellCode[1]);
            nativeStream.WriteObject(dllmain);
            nativeStream.WriteBytes(shellCode[2]);

            //shellCodeMem.WriteBytes(shellCode[0]);
            //shellCodeMem += shellCode[0].Length;
            //
            //Marshal.StructureToPtr(remoteMem, shellCodeMem, false);
            //shellCodeMem += ptrSize;
            //
            //shellCodeMem.WriteBytes(shellCode[1]);
            //shellCodeMem += shellCode[1].Length;
            //
            //Marshal.StructureToPtr(dllmain, shellCodeMem, false);
            //shellCodeMem += ptrSize;
            //
            //shellCodeMem.WriteBytes(shellCode[2]);
            //shellCodeMem += shellCode[2].Length;

            var remoteShellCodeMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (remoteShellCodeMem == IntPtr.Zero)
                throw new NativeMemoryException("Remote DllMain caller shellcode memory");

            state = WriteProcessMemory(processHandle, remoteShellCodeMem, shellCodeMemOriginal, (UIntPtr)shellCodeSize, ref written);
            if (!state || written != (UIntPtr)shellCodeSize)
                throw new NativeMemoryException("Remote DllMain caller shellcode memory", remoteShellCodeMem, (UIntPtr)shellCodeSize, written);

            var threadHandle = processHandle.CreateRemoteThreadAuto(remoteShellCodeMem, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero)
                throw new AggregateException("Cannot create remote DllMain caller thread");

            if (WaitForSingleObject(threadHandle, 30000) != 0)
                throw new AggregateException("DllMain caller thread didn't finished successfully");

            VirtualFreeEx(processHandle, remoteShellCodeMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);

            return (localMem, remoteMem);
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
