using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate uint GetModuleFileNameExProc(IntPtr hProcess,
            IntPtr hModule,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize);
        internal static uint GetModuleFileNameEx(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize) => Lookup<GetModuleFileNameExProc>("psapi.dll", "GetModuleFileNameExW")(hProcess, hModule, lpBaseName, nSize);
    }
}
