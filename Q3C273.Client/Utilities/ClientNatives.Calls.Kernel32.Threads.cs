using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate int GetThreadIdProc(IntPtr threadHandle);
        internal static int GetThreadId(IntPtr threadHandle) => Lookup<GetThreadIdProc>("kernel32.dll", "GetThreadId")(threadHandle);

        internal delegate uint GetCurrentThreadIdProc();
        internal static uint GetCurrentThreadId() => Lookup<GetCurrentThreadIdProc>("kernel32.dll", "GetCurrentThreadId")();

        internal delegate IntPtr GetCurrentThreadProc();
        internal static IntPtr GetCurrentThread() => Lookup<GetCurrentThreadProc>("kernel32.dll", "GetCurrentThread")();

        internal delegate IntPtr OpenThreadProc(
            [In] ThreadAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId);
        internal static IntPtr OpenThread(
            [In] ThreadAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId)
            => Lookup<OpenThreadProc>("kernel32.dll", "OpenThread")(dwDesiredAccess, bInheritHandle, dwProcessId);

        internal delegate bool TerminateThreadProc(IntPtr hThread, uint dwExitCode);
        internal static bool TerminateThread(IntPtr hThread, uint dwExitCode) => Lookup<TerminateThreadProc>("kernel32.dll", "TerminateThread")(hThread, dwExitCode);

        internal delegate bool OpenThreadTokenProc(
            [In] IntPtr hThread,
            [In] ThreadTokenAccessRights dwDesiredAccess,
            [In] bool bOpenAsSelf,
            out IntPtr hToken);
        internal static bool OpenThreadToken(
            [In] IntPtr hThread,
            [In] ThreadTokenAccessRights dwDesiredAccess,
            [In] bool bOpenAsSelf,
            [Out] out IntPtr hToken) => Lookup<OpenThreadTokenProc>("kernel32.dll", "OpenThreadToken")(hThread, dwDesiredAccess, bOpenAsSelf, out hToken);

        internal delegate IntPtr CreateRemoteThreadProc(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            UIntPtr dwStackSize,
            IntPtr lpStartAdress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            ref uint lpThreadId);
        internal static IntPtr CreateRemoteThread(
                    IntPtr hProcess,
                    IntPtr lpThreadAttributes,
                    UIntPtr dwStackSize,
                    IntPtr lpStartAdress,
                    IntPtr lpParameter,
                    uint dwCreationFlags,
                    ref uint lpThreadId) => Lookup<CreateRemoteThreadProc>("kernel32.dll", "CreateRemoteThread")(hProcess, lpThreadAttributes, dwStackSize, lpStartAdress, lpParameter, dwCreationFlags, ref lpThreadId);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool GetExitCodeThreadProc(
            IntPtr hThread,
            out uint lpExitCode);
        internal static bool GetExitCodeThread(
                    IntPtr hThread,
                    out uint lpExitCode) => Lookup<GetExitCodeThreadProc>("kernel32.dll", "GetExitCodeThread")(hThread, out lpExitCode);
    }
}
