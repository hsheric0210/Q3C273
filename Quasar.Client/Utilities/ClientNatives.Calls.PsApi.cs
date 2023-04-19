using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Utilities
{
    internal static partial class ClientNatives
    {
        //[DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal delegate uint GetModuleFileNameExProc(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize);
        internal static uint GetModuleFileNameEx(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize) => Lookup<GetModuleFileNameExProc>("psapi.dll", "GetModuleFileNameEx")(hProcess, hModule, lpBaseName, nSize);
    }
}
