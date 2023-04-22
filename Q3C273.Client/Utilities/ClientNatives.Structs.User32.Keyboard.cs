// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        //values from Winuser.h in Microsoft SDK.
        public const byte VK_SHIFT = 0x10;

        public const byte VK_CAPITAL = 0x14;
        public const byte VK_NUMLOCK = 0x90;
        public const byte VK_LSHIFT = 0xA0;
        public const byte VK_RSHIFT = 0xA1;
        public const byte VK_LCONTROL = 0xA2;
        public const byte VK_RCONTROL = 0xA3;
        public const byte VK_LMENU = 0xA4;
        public const byte VK_RMENU = 0xA5;
        public const byte VK_LWIN = 0x5B;
        public const byte VK_RWIN = 0x5C;
        public const byte VK_SCROLL = 0x91;

        public const byte VK_INSERT = 0x2D;

        //may be possible to use these aggregates instead of L and R separately (untested)
        public const byte VK_CONTROL = 0x11;

        public const byte VK_MENU = 0x12;

        public const byte VK_PACKET = 0xE7;

        //Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods
        private static int lastVirtualKeyCode;

        private static int lastScanCode;
        private static byte[] lastKeyState = new byte[255];
        private static bool lastIsDead;

        /// <summary>
        ///     MapVirtualKeys uMapType
        /// </summary>
        internal enum MapType
        {
            /// <summary>
            ///     uCode is a virtual-key code and is translated into an unshifted character value in the low-order word of the return
            ///     value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation,
            ///     the function returns 0.
            /// </summary>
            MAPVK_VK_TO_VSC,

            /// <summary>
            ///     uCode is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not
            ///     distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the
            ///     function returns 0.
            /// </summary>
            MAPVK_VSC_TO_VK,

            /// <summary>
            ///     uCode is a scan code and is translated into a virtual-key code that does not distinguish between left- and
            ///     right-hand keys. If there is no translation, the function returns 0.
            /// </summary>
            MAPVK_VK_TO_CHAR,

            /// <summary>
            ///     uCode is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand
            ///     keys. If there is no translation, the function returns 0.
            /// </summary>
            MAPVK_VSC_TO_VK_EX
        }
    }
}