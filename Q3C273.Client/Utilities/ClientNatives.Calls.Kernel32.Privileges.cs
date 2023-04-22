using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool LookupPrivilegeValueProc(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpSystemName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpName,
            IntPtr pLuid);
        internal static bool LookupPrivilegeValue(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpSystemName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpName,
            IntPtr pLuid) => Lookup<LookupPrivilegeValueProc>("kernel32.dll", "LookupPrivilegeValueW")(lpSystemName, lpName, pLuid);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool AdjustTokenPrivilegesProc(
            [In] IntPtr hToken,
            [In, MarshalAs(UnmanagedType.Bool)] bool bDisableAllPrivileges,
            [In] IntPtr pNewState,
            [In] int dwBufferState,
            IntPtr pPreviousState,
            IntPtr dwReturnLength);
        internal static bool AdjustTokenPrivileges(
            [In] IntPtr hToken,
            [In, MarshalAs(UnmanagedType.Bool)] bool bDisableAllPrivileges,
            [In] IntPtr pNewState,
            [In] int dwBufferState,
            IntPtr pPreviousState,
            IntPtr dwReturnLength) => Lookup<AdjustTokenPrivilegesProc>("kernel32.dll", "AdjustTokenPrivileges")(hToken, bDisableAllPrivileges, pNewState, dwBufferState, pPreviousState, dwReturnLength);

    }
}
