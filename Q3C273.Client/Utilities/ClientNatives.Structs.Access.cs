using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        internal enum SecurityImpersonationLevel
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        internal enum PrivilegeAttributes
        {
            SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
            SE_PRIVILEGE_ENABLED = 0x00000002,
            SE_PRIVILEGE_REMOVED = 0X00000004
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
            public LUID_AND_ATTRIBUTES Privileges;
        }
    }
}
