using System;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        // Memory Protection Constants
        // https://docs.microsoft.com/en-us/windows/win32/memory/memory-protection-constants
        [Flags]
        public enum PageAccessRights : uint
        {
            None = 0,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,

            // Modifier flags

            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        [Flags]
        public enum AllocationType : uint
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags]
        public enum MemFreeType : uint
        {
            None = 0x0,
            MEM_COALESCE_PLACEHOLDERS = 0x01,
            MEM_PRESERVE_PLACEHOLDER = 0x02,
            MEM_DECOMMIT = 0x00004000,
            MEM_RELEASE = 0x00008000,
        }
    }
}
