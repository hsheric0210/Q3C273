using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate bool QueryFullProcessImageNameFunc([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        internal static bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize) => Lookup<QueryFullProcessImageNameFunc>("kernel32.dll", "QueryFullProcessImageName")(hProcess, dwFlags, lpExeName, ref lpdwSize);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool IsWow64ProcessProc([In] IntPtr hProcess, [Out] out bool lpWow64Process);
        internal static bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpWow64Process) => Lookup<IsWow64ProcessProc>("kernel32.dll", "IsWow64Process")(hProcess, out lpWow64Process);

        internal delegate IntPtr OpenProcessProc(
            [In] ProcessAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId);
        internal static IntPtr OpenProcess(
            [In] ProcessAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId)
            => Lookup<OpenProcessProc>("kernel32.dll", "OpenProcess")(dwDesiredAccess, bInheritHandle, dwProcessId);

        internal delegate IntPtr GetCurrentProcessProc();
        internal static IntPtr GetCurrentProcess() => Lookup<GetCurrentProcessProc>("kernel32.dll", "GetCurrentProcess")();

        internal delegate int GetCurrentProcessIdProc();
        internal static int GetCurrentProcessId() => Lookup<GetCurrentProcessIdProc>("kernel32.dll", "GetCurrentProcessId")();

        internal delegate int GetProcessIdProc([In] IntPtr Process);
        internal static int GetProcessId([In] IntPtr Process) => Lookup<GetProcessIdProc>("kernel32.dll", "GetProcessId")(Process);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool WriteProcessMemoryProc(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UIntPtr nSize,
            ref UIntPtr lpNumberOfBytesWritten);
        internal static bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UIntPtr nSize,
            ref UIntPtr lpNumberOfBytesWritten) => Lookup<WriteProcessMemoryProc>("kernel32.dll", "WriteProcessMemory")(hProcess, lpBaseAddress, lpBuffer, nSize, ref lpNumberOfBytesWritten);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool ReadProcessMemoryProc(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UIntPtr nSize,
            ref UIntPtr lpNumberOfBytesRead);
        internal static bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UIntPtr nSize,
            ref UIntPtr lpNumberOfBytesRead) => Lookup<WriteProcessMemoryProc>("kernel32.dll", "ReadProcessMemory")(hProcess, lpBaseAddress, lpBuffer, nSize, ref lpNumberOfBytesRead);


    }
}
