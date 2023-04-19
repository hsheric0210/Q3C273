// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Text;
using System.Windows.Forms;
using Ton618.MouseKeyHook.Implementation;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        /// <summary>
        ///     Translates a virtual key to its character equivalent using the current keyboard layout without knowing the
        ///     scancode in advance.
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="fuState"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int fuState, out char[] chars)
        {
            var dwhkl = GetActiveKeyboard();
            var scanCode = MapVirtualKeyEx(virtualKeyCode, (int)MapType.MAPVK_VK_TO_VSC, dwhkl);
            TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, dwhkl, out chars);
        }

        /// <summary>
        ///     Translates a virtual key to its character equivalent using the current keyboard layout
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="scanCode"></param>
        /// <param name="fuState"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState,
            out char[] chars)
        {
            var dwhkl = GetActiveKeyboard(); //get the active keyboard layout
            TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, dwhkl, out chars);
        }

        /// <summary>
        ///     Translates a virtual key to its character equivalent using a specified keyboard layout
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="scanCode"></param>
        /// <param name="fuState"></param>
        /// <param name="dwhkl"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState, IntPtr dwhkl,
            out char[] chars)
        {
            var pwszBuff = new StringBuilder(64);
            var keyboardState = KeyboardState.GetCurrent();
            var currentKeyboardState = keyboardState.GetNativeState();
            var isDead = false;

            if (keyboardState.IsDown(Keys.ShiftKey))
                currentKeyboardState[(byte)Keys.ShiftKey] = 0x80;

            if (keyboardState.IsToggled(Keys.CapsLock))
                currentKeyboardState[(byte)Keys.CapsLock] = 0x01;

            var relevantChars = ToUnicodeEx(virtualKeyCode, scanCode, currentKeyboardState, pwszBuff, pwszBuff.Capacity,
                fuState, dwhkl);

            switch (relevantChars)
            {
                case -1:
                    isDead = true;
                    ClearKeyboardBuffer(virtualKeyCode, scanCode, dwhkl);
                    chars = null;
                    break;

                case 0:
                    chars = null;
                    break;

                case 1:
                    if (pwszBuff.Length > 0)
                        chars = new[] { pwszBuff[0] };
                    else
                        chars = null;
                    break;

                // Two or more (only two of them is relevant)
                default:
                    if (pwszBuff.Length > 1)
                        chars = new[] { pwszBuff[0], pwszBuff[1] };
                    else
                        chars = new[] { pwszBuff[0] };
                    break;
            }

            if (lastVirtualKeyCode != 0 && lastIsDead)
            {
                if (chars != null)
                {
                    var sbTemp = new StringBuilder(5);
                    ToUnicodeEx(lastVirtualKeyCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, 0, dwhkl);
                    lastIsDead = false;
                    lastVirtualKeyCode = 0;
                }

                return;
            }

            lastScanCode = scanCode;
            lastVirtualKeyCode = virtualKeyCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])currentKeyboardState.Clone();
        }

        private static void ClearKeyboardBuffer(int vk, int sc, IntPtr hkl)
        {
            var sb = new StringBuilder(10);

            int rc;
            do
            {
                var lpKeyStateNull = new byte[255];
                rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }

        /// <summary>
        ///     Gets the input locale identifier for the active application's thread.  Using this combined with the ToUnicodeEx and
        ///     MapVirtualKeyEx enables Windows to properly translate keys based on the keyboard layout designated for the
        ///     application.
        /// </summary>
        /// <returns>HKL</returns>
        private static IntPtr GetActiveKeyboard()
        {
            var hActiveWnd = GetForegroundWindow(); //handle to focused window
            int dwProcessId;
            var hCurrentWnd = GetWindowThreadProcessId(hActiveWnd, out dwProcessId);
            //thread of focused window
            return GetKeyboardLayout(hCurrentWnd); //get the layout identifier for the thread whose window is focused
        }
    }
}