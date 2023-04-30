using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        public delegate IntPtr GlobalLockProc([In] IntPtr hMem);
        public static IntPtr GlobalLock([In] IntPtr hMem) => Lookup<GlobalLockProc>("kernel32.dll", "GlobalLock")(hMem);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool GlobalUnlockProc([In] IntPtr hMem);
        public static bool GlobalUnlock([In] IntPtr hMem) => Lookup<GlobalUnlockProc>("kernel32.dll", "GlobalUnlock")(hMem);
    }
}
