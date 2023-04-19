// This code is distributed under MIT license.
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

namespace Everything.Utilities
{
    // Because it is a P/Invoke method, 'GetSystemMetrics(int)'
    // should be defined in a class named NativeMethods, SafeNativeMethods,
    // or UnsafeClientNatives.
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724385(v=vs.85).aspx
    internal static partial class ClientNatives
    {
        private const int SM_SWAPBUTTON = 23;
        private const int SM_CXDRAG = 68;
        private const int SM_CYDRAG = 69;
        private const int SM_CXDOUBLECLK = 36;
        private const int SM_CYDOUBLECLK = 37;

        private delegate int GetSystemMetricsProc(int index);
        private static int GetSystemMetrics(int index) => ClientNatives.Lookup<GetSystemMetricsProc>("user32.dll", "GetSystemMetrics")(index);

        public static int GetSwapButtonThreshold() => GetSystemMetrics(SM_SWAPBUTTON);

        public static int GetXDragThreshold() => GetSystemMetrics(SM_CXDRAG) / 2 + 1;

        public static int GetYDragThreshold() => GetSystemMetrics(SM_CYDRAG) / 2 + 1;

        public static int GetXDoubleClickThreshold() => GetSystemMetrics(SM_CXDOUBLECLK) / 2 + 1;

        public static int GetYDoubleClickThreshold() => GetSystemMetrics(SM_CYDOUBLECLK) / 2 + 1;
    }
}
