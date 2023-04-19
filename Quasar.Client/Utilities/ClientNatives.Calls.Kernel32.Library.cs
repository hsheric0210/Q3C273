using System;
using System.Runtime.InteropServices;

namespace Everything.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate IntPtr LoadLibraryFunc([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
        internal static IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName) => Lookup<LoadLibraryFunc>("kernel32.dll", "LoadLibraryW")(lpFileName);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool FreeLibraryFunc(IntPtr hModule);
        internal static bool FreeLibrary(IntPtr hModule) => Lookup<FreeLibraryFunc>("kernel32.dll", "FreeLibrary")(hModule);
    }
}
