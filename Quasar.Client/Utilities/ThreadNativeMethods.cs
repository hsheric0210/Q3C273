// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Runtime.InteropServices;

namespace Quasar.Client.Utilities
{
    internal static class ThreadNativeMethods
    {
        internal delegate int GetCurrentThreadIdProc();
        /// <summary>
        ///     Retrieves the unmanaged thread identifier of the calling thread.
        /// </summary>
        /// <returns></returns>
        //[DllImport("kernel32")]
        internal static int GetCurrentThreadId() => ClientNatives.Lookup<GetCurrentThreadIdProc>("kernel32.dll", "GetCurrentThreadId")();

        internal delegate IntPtr GetForegroundWindowProc();
        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is currently working).
        ///     The system assigns a slightly higher priority to the thread that creates the foreground window than it does to
        ///     other threads.
        /// </summary>
        /// <returns></returns>
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static IntPtr GetForegroundWindow() => ClientNatives.Lookup<GetForegroundWindowProc>("user32.dll", "GetForegroundWindow")();

        internal delegate int GetWindowThreadProcessIdProc(IntPtr handle, out int processId);
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
        internal static int GetWindowThreadProcessId(IntPtr handle, out int processId) => ClientNatives.Lookup<GetWindowThreadProcessIdProc>("user32.dll", "GetWindowThreadProcessId")(handle, out processId);
    }
}