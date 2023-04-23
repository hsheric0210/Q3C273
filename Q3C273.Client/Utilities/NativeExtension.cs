using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities
{
    internal static class NativeExtension
    {
        public static IntPtr WriteToProcess(this IntPtr processHandle, IntPtr localMemory, UIntPtr sizeOfData, PageAccessRights protect = PageAccessRights.PAGE_READWRITE)
        {
            var remoteAddress = VirtualAllocEx(processHandle, IntPtr.Zero, sizeOfData, AllocationType.COMMIT | AllocationType.RESERVE, protect);
            if (remoteAddress == IntPtr.Zero)
                throw new NativeMemoryException("Remote process memory");

            var written = UIntPtr.Zero;
            var state = WriteProcessMemory(processHandle, remoteAddress, localMemory, sizeOfData, ref written);
            if (!state || written != sizeOfData)
                throw new NativeMemoryException("Remote process memory", remoteAddress, sizeOfData, written);
            return remoteAddress;
        }

        public static IntPtr WriteStringToProcess(this IntPtr processHandle, IntPtr localStringMemory, int stringLength) =>
            // +1 for the null termination char
            processHandle.WriteToProcess(localStringMemory, (UIntPtr)(stringLength + 1));

        public static IntPtr WriteStringToProcess(this IntPtr processHandle, IntPtr localStringMemory, out int stringLength)
            => processHandle.WriteStringToProcess(localStringMemory, stringLength = Marshal.PtrToStringAnsi(localStringMemory).Length);

        public unsafe static IntPtr GetProcAddressRemote(this IntPtr processHandle, IntPtr dllHandle, IntPtr procNameOrOrdinal, bool byOrdinal, bool is64bit)
        {
            bool state;
            var written = UIntPtr.Zero;
            var ptrSize = Marshal.SizeOf<IntPtr>();

            IntPtr procNameMemory;
            if (byOrdinal)
            {
                procNameMemory = procNameOrOrdinal;
            }
            else
            {
                procNameMemory = processHandle.WriteStringToProcess(procNameOrOrdinal, out _);
                /*
                var procNameString = Marshal.PtrToStringAnsi(procNameOrOrdinal);
                var procNameSize = (UIntPtr)(procNameString.Length + 1);
                procNameMemory = VirtualAllocEx(processHandle, IntPtr.Zero, procNameSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (procNameMemory == IntPtr.Zero)
                    throw new NativeMemoryException("Function name memory");

                state = WriteProcessMemory(processHandle, procNameMemory, procNameOrOrdinal, procNameSize, ref written);
                if (!state || procNameSize != written)
                    throw new NativeMemoryException("Function name memory", procNameMemory, procNameSize, written);
                */
            }

            var resultBufferMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)ptrSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
            if (resultBufferMemory == IntPtr.Zero)
                throw new NativeMemoryException("Remote GetProcAddress result buffer");

            var getProcAddress = LookupPointer("kernel32.dll", "GetProcAddress");

            (is64bit ? ShellCode.GetProcAddress_x64 : ShellCode.GetProcAddress_x86)
                .ExecuteOn(
                    processHandle,
                    dllHandle,
                    procNameMemory,
                    getProcAddress,
                    resultBufferMemory);
            /*
            #region GetProcAddress ShellCode
            byte[][] shellCode;
            if (is64bit)
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
            var nativeStream = new UnmanagedMemoryStream((byte*)shellCodeMem.ToPointer(), shellCodeSize, shellCodeSize, FileAccess.ReadWrite);
            nativeStream.WriteBytes(shellCode[0]);
            nativeStream.WriteObject(dllHandle);
            nativeStream.WriteBytes(shellCode[1]);
            nativeStream.WriteObject(procNameMemory);
            nativeStream.WriteBytes(shellCode[2]);
            nativeStream.WriteObject(getProcAddress);
            nativeStream.WriteBytes(shellCode[3]);
            nativeStream.WriteObject(resultBufferMemory);
            nativeStream.WriteBytes(shellCode[4]);

            var remoteShellCodeMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
            if (remoteShellCodeMemory == IntPtr.Zero)
                throw new NativeMemoryException("Remote GetProcAddress shell code memory");

            state = WriteProcessMemory(processHandle, remoteShellCodeMemory, shellCodeMem, (UIntPtr)shellCodeSize, ref written);
            if (!state || written != (UIntPtr)shellCodeSize)
                throw new NativeMemoryException("Remote GetProcAddress shell code memory", remoteShellCodeMemory, (UIntPtr)shellCodeSize, written);

            var threadHandle = CreateRemoteThreadAuto(processHandle, remoteShellCodeMemory, IntPtr.Zero);
            if (WaitForSingleObject(threadHandle, 30000) != 0)
                throw new AggregateException("GetProcAddress remote thread didn't finished successfully.");

            #endregion
            */
            var retBuffer = Marshal.AllocHGlobal(ptrSize);
            state = ReadProcessMemory(processHandle, resultBufferMemory, retBuffer, (UIntPtr)ptrSize, ref written);
            if (!state)
                throw new NativeMemoryException("Remote GetProcAddress shell code memory", resultBufferMemory);

            var procAddress = Marshal.PtrToStructure<IntPtr>(retBuffer);

            //VirtualFreeEx(processHandle, remoteShellCodeMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            VirtualFreeEx(processHandle, resultBufferMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            if (!byOrdinal)
                VirtualFreeEx(processHandle, procNameMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            Marshal.FreeHGlobal(retBuffer);

            return procAddress;
        }

        public static IntPtr CreateRemoteThreadAuto(this IntPtr processHandle, IntPtr startAddress, IntPtr parameterAddress)
        {
            var threadHandle = IntPtr.Zero;

            if (Environment.OSVersion.Version > new Version(6, 0) && Environment.OSVersion.Version < new Version(6, 2)) // Windows Vista and 7
            {
                var retVal = NtCreateThreadEx(ref threadHandle, 0x1FFFFF, IntPtr.Zero, processHandle, startAddress, parameterAddress, false, 0, 0xFFFF, 0xFFFF, IntPtr.Zero);
                if (threadHandle == IntPtr.Zero)
                    throw new AggregateException("NtCreateThreadEx failed. Return=" + retVal + ", Win32Error=" + GetLastError());
            }
            else
            {
                var tid = 0u;
                threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, UIntPtr.Zero, startAddress, parameterAddress, 0, ref tid);
                if (threadHandle == IntPtr.Zero)
                    throw new AggregateException("CreateRemoteThread failed. Win32Error=" + GetLastError());
            }
            return threadHandle;
        }
    }
}
