using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {

        internal delegate bool OpenThreadTokenProc(
            [In] IntPtr hThread,
            [In] ThreadTokenAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bOpenAsSelf,
            out IntPtr hToken);
        internal static bool OpenThreadToken(
            [In] IntPtr hThread,
            [In] ThreadTokenAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bOpenAsSelf,
            out IntPtr hToken) => Lookup<OpenThreadTokenProc>("advapi32.dll", "OpenThreadToken")(hThread, dwDesiredAccess, bOpenAsSelf, out hToken);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool ImpersonateSelfProc([In] SecurityImpersonationLevel ImpersonationLevel);
        internal static bool ImpersonateSelf([In] SecurityImpersonationLevel ImpersonationLevel) => Lookup<ImpersonateSelfProc>("advapi32.dll", "ImpersonateSelf")(ImpersonationLevel);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool LookupPrivilegeValueProc(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpSystemName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpName,
            IntPtr pLuid);
        internal static bool LookupPrivilegeValue(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpSystemName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpName,
            IntPtr pLuid) => Lookup<LookupPrivilegeValueProc>("advapi32.dll", "LookupPrivilegeValueW")(lpSystemName, lpName, pLuid);

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
            IntPtr dwReturnLength) => Lookup<AdjustTokenPrivilegesProc>("advapi32.dll", "AdjustTokenPrivileges")(hToken, bDisableAllPrivileges, pNewState, dwBufferState, pPreviousState, dwReturnLength);
    }
}
