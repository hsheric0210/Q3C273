﻿using System;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate IntPtr VirtualAllocProc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, PageAccessRights flProtect);
        internal static IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, PageAccessRights flProtect)
            => Lookup<VirtualAllocProc>("kernel32.dll", "VirtualAlloc")(lpAddress, dwSize, flAllocationType, flProtect);

        internal delegate IntPtr VirtualAllocExProc(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, PageAccessRights flProtect);
        internal static IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, PageAccessRights flProtect)
            => Lookup<VirtualAllocExProc>("kernel32.dll", "VirtualAllocEx")(hProcess, lpAddress, dwSize, flAllocationType, flProtect);

        internal delegate bool VirtualFreeProc(IntPtr lpAddress, UIntPtr dwSize, MemFreeType dwFreeType);
        internal static bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, MemFreeType dwFreeType)
            => Lookup<VirtualFreeProc>("kernel32.dll", "VirtualFree")(lpAddress, dwSize, dwFreeType);

        internal delegate bool VirtualFreeExProc(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, MemFreeType dwFreeType);
        internal static bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, MemFreeType dwFreeType)
            => Lookup<VirtualFreeExProc>("kernel32.dll", "VirtualFreeEx")(hProcess, lpAddress, dwSize, dwFreeType);

        internal delegate bool VirtualProtectProc(
            IntPtr lpAddress,
            UIntPtr dwSize,
            PageAccessRights flNewProtect,
            out PageAccessRights lpflOldProtect);
        internal static bool VirtualProtect(
            IntPtr lpAddress,
            UIntPtr dwSize,
            PageAccessRights flNewProtect,
            out PageAccessRights lpflOldProtect)
            => Lookup<VirtualProtectProc>("kernel32.dll", "VirtualProtect")(lpAddress, dwSize, flNewProtect, out lpflOldProtect);

        internal delegate bool VirtualProtectExProc(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            PageAccessRights flNewProtect,
            out PageAccessRights lpflOldProtect);
        internal static bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            PageAccessRights flNewProtect,
            out PageAccessRights lpflOldProtect)
            => Lookup<VirtualProtectExProc>("kernel32.dll", "VirtualProtectEx")(hProcess, lpAddress, dwSize, flNewProtect, out lpflOldProtect);
    }
}
