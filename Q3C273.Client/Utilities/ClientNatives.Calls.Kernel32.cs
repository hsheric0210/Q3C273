using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Ton618.Win32PE.Structs;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private static readonly StringBuilder m_sbSysMsg = new StringBuilder(1024);

        public const int MAX_PATH = 260;

        public static string FormatSystemMessage(int dwMessageId)
        {
            lock (m_sbSysMsg)
            {
                var ret = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM,
                                        IntPtr.Zero,
                                        dwMessageId,
                                        CultureInfo.CurrentCulture.LCID,
                                        m_sbSysMsg,
                                        m_sbSysMsg.Capacity - 1,
                                        IntPtr.Zero);

                if (ret > 0)
                    return m_sbSysMsg.ToString(0, ret);
                return null;
            }
        }

        internal delegate int FormatMessageProc(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
            int nSize,
            IntPtr lpArguments);
        internal static int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
            int nSize,
            IntPtr lpArguments) => Lookup<FormatMessageProc>("kernel32.dll", "FormatMessageW")(dwFlags, lpSource, dwMessageId, dwLanguageId, lpBuffer, nSize, lpArguments);

        internal delegate int QueryDosDeviceProc(
            [In] string lpDeviceName,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpTargetPath,
            [In] int ucchMax);
        internal static int QueryDosDevice(
            [In] string lpDeviceName,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpTargetPath,
            [In] int ucchMax) => Lookup<QueryDosDeviceProc>("kernel32.dll", "QueryDosDeviceW")(lpDeviceName, lpTargetPath, ucchMax);

        internal delegate void GetSystemInfoProc(ref SYSTEM_INFO Info);
        internal static void GetSystemInfo(ref SYSTEM_INFO Info) => Lookup<GetSystemInfoProc>("kernel32.dll", "GetSystemInfo")(ref Info);

        internal delegate bool FlushInstructionCacheProc(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
        internal static bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize)
            => Lookup<FlushInstructionCacheProc>("kernel32.dll", "FlushInstructionCache")(hProcess, lpBaseAddress, dwSize);
    }
}
