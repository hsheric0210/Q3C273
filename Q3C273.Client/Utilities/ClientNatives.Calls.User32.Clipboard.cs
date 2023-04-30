using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool OpenClipboardProc([In, Optional] IntPtr hWnd);
        public static bool OpenClipboard([In, Optional] IntPtr hWnd) => Lookup<OpenClipboardProc>("user32.dll", "OpenClipboard")(hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool CloseClipboardProc();
        public static bool CloseClipboard() => Lookup<CloseClipboardProc>("user32.dll", "CloseClipboard")();

        public delegate uint GetPriorityClipboardFormatProc([In] IntPtr priorityList, [In] uint formats);
        public static uint GetPriorityClipboardFormat([In] IntPtr priorityList, [In] uint formats) => Lookup<GetPriorityClipboardFormatProc>("user32.dll", "GetPriorityClipboardFormat")(priorityList, formats);

        public delegate IntPtr GetClipboardDataProc([In] uint format);
        public static IntPtr GetClipboardData([In] uint format) => Lookup<GetClipboardDataProc>("user32.dll", "GetclipboardData")(format);

        public delegate uint GetClipboardSequenceNumberProc();
        public static uint GetClipboardSequenceNumber() => Lookup<GetClipboardSequenceNumberProc>("user32.dll", "GetClipboardSequenceNumber")();
    }
}
