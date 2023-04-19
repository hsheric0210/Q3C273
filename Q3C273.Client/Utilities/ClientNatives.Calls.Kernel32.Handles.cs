using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Ton618.Win32PE.Structs;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool DuplicateHandleProc(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions);
        internal static bool DuplicateHandle(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions) => Lookup<DuplicateHandleProc>("kernel32.dll", "DuplicateHandle")(
                hSourceProcessHandle,
                hSourceHandle,
                hTargetProcessHandle,
                out lpTargetHandle,
                dwDesiredAccess,
                bInheritHandle,
                dwOptions);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool CloseHandleProc([In] IntPtr hObject);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static bool CloseHandle([In] IntPtr hObject) => Lookup<CloseHandleProc>("kernel32.dll", "CloseHandle")(hObject);
    }
}
