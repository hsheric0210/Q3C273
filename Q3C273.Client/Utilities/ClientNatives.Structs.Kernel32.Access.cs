using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        internal enum PrivilegeAttributes : uint
        {
            SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001u,
            SE_PRIVILEGE_ENABLED = 0x00000002u,
            SE_PRIVILEGE_REMOVED = 0X00000004u
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public uint LowPart;
            public uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }


        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        public enum ThreadAccessRights : uint
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            THREAD_QUERY_INFORMATION = 0x00000040,

            // Vista or later
            THREAD_ALL_ACCESS = 0xFFFF | NativeFileAccess.SYNCHRONIZE | NativeFileAccess.STANDARD_RIGHTS_REQUIRED,
            // XP or below
            // THREAD_ALL_ACCESS = (uint)0x3FF | NativeFileAccess.SYNCHRONIZE | NativeFileAccess.STANDARD_RIGHTS_REQUIRED,
        }

        [Flags]
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute
        public enum ThreadTokenAccessRights : uint
#pragma warning restore CA2217 // Do not mark enums with FlagsAttribute
        {
            TOKEN_QUERY = 0x0008,
            TOKEN_ADJUST_PRIVILEGES = 0x0020
        }

        [Flags]
        public enum ProcessAccessRights : uint
        {
            PROCESS_ALL_ACCESS = 0x000F0000u | 0x00100000u | 0xFFFF,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x10,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_DUP_HANDLE = 0x00000040,
            PROCESS_QUERY_INFORMATION = 0x0400,
        }
    }
}
