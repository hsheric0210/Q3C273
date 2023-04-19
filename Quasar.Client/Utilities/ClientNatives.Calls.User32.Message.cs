using System;

namespace Quasar.Client.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        //[DllImport("user32.dll", SetLastError = false)]
        internal delegate IntPtr GetMessageExtraInfoFunc();
        internal static IntPtr GetMessageExtraInfo() => Lookup<GetMessageExtraInfoFunc>("user32.dll", "GetMessageExtraInfo")();

        //[DllImport("user32.dll")]
        internal delegate int PostMessageFunc(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        internal static int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam) => Lookup<PostMessageFunc>("user32.dll", "PostMessage")(hWnd, wMsg, wParam, lParam);
    }
}
