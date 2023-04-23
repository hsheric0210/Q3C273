using Q3C273.Shared;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        private unsafe IntPtr ImportDllInRemoteProcess(IntPtr processHandle, IntPtr importDllPathPtr)
        {
            var written = UIntPtr.Zero;
            var ptrSize = Marshal.SizeOf<IntPtr>();

            var dllPathString = Marshal.PtrToStringAnsi(importDllPathPtr);
            var dllPathSize = (UIntPtr)(dllPathString.Length + 1);
            var remoteDllPathMemory = VirtualAllocEx(processHandle, IntPtr.Zero, dllPathSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
            if (remoteDllPathMemory == IntPtr.Zero)
                throw new NativeMemoryException("Module path parameter memory for remote LoadLibraryA call");

            var state = WriteProcessMemory(processHandle, remoteDllPathMemory, importDllPathPtr, dllPathSize, ref written);
            if (!state || written != dllPathSize)
                throw new NativeMemoryException("Module path parameter memory for remote LoadLibraryA call", remoteDllPathMemory, dllPathSize, written);

            var loadLibraryProcAddress = LookupPointer("kernel32.dll", "LoadLibraryA");

            IntPtr dllAddress;
            if (Is64Bitness)
            {
                //For 64bit DLL's, we can't use just CreateRemoteThread to call LoadLibrary because GetExitCodeThread will only give back a 32bit value, but we need a 64bit address
                //Instead, write shellcode while calls LoadLibrary and writes the result to a memory address we specify. Then read from that memory once the thread finishes.
                var returnValueMemory = VirtualAllocEx(processHandle, IntPtr.Zero, dllPathSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (returnValueMemory == IntPtr.Zero)
                    throw new NativeMemoryException("LoadLibraryA return value memory");

                #region SHELLCODE
                // Write Shellcode to the remote process which will call LoadLibraryA (Shellcode: LoadLibraryA.asm)
                var shellCode = new byte[][]
                {
                    new byte[] { 0x53, 0x48, 0x89, 0xe3, 0x48, 0x83, 0xec, 0x20, 0x66, 0x83, 0xe4, 0xc0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0xba },
                    new byte[] { 0xff, 0xd2, 0x48, 0xba },
                    new byte[] { 0x48, 0x89, 0x02, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                };
                var shellCodeSize = shellCode.Sum(s => s.Length) + ptrSize * 3;

                var shellCodeMem = Marshal.AllocHGlobal(shellCodeSize);
                var nativeStream = new UnmanagedMemoryStream((byte*)shellCodeMem.ToPointer(), shellCodeSize, shellCodeSize, FileAccess.ReadWrite);
                nativeStream.WriteBytes(shellCode[0]);
                nativeStream.WriteObject(remoteDllPathMemory);
                nativeStream.WriteBytes(shellCode[1]);
                nativeStream.WriteObject(loadLibraryProcAddress);
                nativeStream.WriteBytes(shellCode[2]);
                nativeStream.WriteObject(returnValueMemory);
                nativeStream.WriteBytes(shellCode[3]);
                #endregion

                var remoteShellCodeMem = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)shellCodeSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_EXECUTE_READWRITE);
                if (remoteShellCodeMem == IntPtr.Zero)
                    throw new NativeMemoryException("Remote memory for LoadLibraryA shellcode");

                state = WriteProcessMemory(processHandle, remoteShellCodeMem, shellCodeMem, (UIntPtr)shellCodeSize, ref written);
                if (!state || (UIntPtr)shellCodeSize != written)
                    throw new NativeMemoryException("Remote memory for LoadLibraryA shellcode", remoteShellCodeMem, (UIntPtr)shellCodeSize, written);

                var threadHandle = processHandle.CreateRemoteThreadAuto(remoteShellCodeMem, remoteDllPathMemory);
                if (WaitForSingleObject(threadHandle, 30000) != 0)
                    throw new AggregateException("Library loader thread didn't finished successfully. (or timeout)");

                var retValMem = Marshal.AllocHGlobal(ptrSize);
                state = ReadProcessMemory(processHandle, returnValueMemory, retValMem, (UIntPtr)ptrSize, ref written);
                if (!state)
                    throw new NativeMemoryException("LoadLibraryA return value memory", returnValueMemory);

                dllAddress = Marshal.PtrToStructure<IntPtr>(retValMem);
                VirtualFreeEx(processHandle, returnValueMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
                VirtualFreeEx(processHandle, remoteShellCodeMem, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            }
            else
            {
                var threadHandle = processHandle.CreateRemoteThreadAuto(loadLibraryProcAddress, remoteDllPathMemory);
                if (WaitForSingleObject(threadHandle, 30000) != 0)
                    throw new AggregateException("Library loader thread didn't finished successfully. (or timeout)");

                state = GetExitCodeThread(threadHandle, out var exitCode);
                if (!state || exitCode == 0)
                    throw new AggregateException("Failed to read library loader thread exit code.");

                dllAddress = (IntPtr)exitCode;
            }

            VirtualFreeEx(processHandle, remoteDllPathMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
            return dllAddress;
        }
    }
}
