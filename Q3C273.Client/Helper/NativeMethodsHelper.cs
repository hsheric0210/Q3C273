using Everything.Utilities;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Everything.Helper
{
    public static class NativeMethodsHelper
    {
        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static uint GetLastInputInfoTickCount()
        {
            var lastInputInfo = new ClientNatives.LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            ClientNatives.GetLastInputInfo(ref lastInputInfo);
            return lastInputInfo.dwTime;
        }

        public static void DoMouseLeftClick(Point p, bool isMouseDown)
        {
            ClientNatives.INPUT[] inputs = {
                new ClientNatives.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new ClientNatives.InputUnion
                    {
                        mi = new ClientNatives.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = 0,
                            dwFlags = isMouseDown ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP,
                            time = 0,
                            dwExtraInfo = ClientNatives.GetMessageExtraInfo()
                        }
                    }
                }
            };

            ClientNatives.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(ClientNatives.INPUT)));
        }

        public static void DoMouseRightClick(Point p, bool isMouseDown)
        {
            ClientNatives.INPUT[] inputs = {
                new ClientNatives.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new ClientNatives.InputUnion
                    {
                        mi = new ClientNatives.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = 0,
                            dwFlags = isMouseDown ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP,
                            time = 0,
                            dwExtraInfo = ClientNatives.GetMessageExtraInfo()
                        }
                    }
                }
            };

            ClientNatives.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(ClientNatives.INPUT)));
        }

        public static void DoMouseMove(Point p)
        {
            ClientNatives.SetCursorPos(p.X, p.Y);
        }

        public static void DoMouseScroll(Point p, bool scrollDown)
        {
            ClientNatives.INPUT[] inputs = {
                new ClientNatives.INPUT
                {
                    type = INPUT_MOUSE,
                    u = new ClientNatives.InputUnion
                    {
                        mi = new ClientNatives.MOUSEINPUT
                        {
                            dx = p.X,
                            dy = p.Y,
                            mouseData = scrollDown ? -120 : 120,
                            dwFlags = MOUSEEVENTF_WHEEL,
                            time = 0,
                            dwExtraInfo = ClientNatives.GetMessageExtraInfo()
                        }
                    }
                }
            };

            ClientNatives.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(ClientNatives.INPUT)));
        }

        public static void DoKeyPress(byte key, bool keyDown)
        {
            ClientNatives.INPUT[] inputs = {
                new ClientNatives.INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new ClientNatives.InputUnion
                    {
                        ki = new ClientNatives.KEYBDINPUT
                        {
                            wVk = key,
                            wScan = 0,
                            dwFlags = keyDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP,
                            dwExtraInfo = ClientNatives.GetMessageExtraInfo()
                        }
                    }
                }
            };

            ClientNatives.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(ClientNatives.INPUT)));
        }

        private const int SPI_GETSCREENSAVERRUNNING = 114;

        public static bool IsScreensaverActive()
        {
            var running = IntPtr.Zero;

            if (!ClientNatives.SystemParametersInfo(
                SPI_GETSCREENSAVERRUNNING,
                0,
                ref running,
                0))
            {
                // Something went wrong (Marshal.GetLastWin32Error)
            }

            return running != IntPtr.Zero;
        }

        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_READOBJECTS = 0x0001;
        private const int WM_CLOSE = 16;
        private const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        private const uint SPIF_SENDWININICHANGE = 0x0002;

        public static void DisableScreensaver()
        {
            var handle = ClientNatives.OpenDesktop("Screen-saver", 0,
                false, DESKTOP_READOBJECTS | DESKTOP_WRITEOBJECTS);

            if (handle != IntPtr.Zero)
            {
                ClientNatives.EnumDesktopWindows(handle, (hWnd, lParam) =>
                {
                    if (ClientNatives.IsWindowVisible(hWnd))
                        ClientNatives.PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    // Continue enumeration even if it fails
                    return true;
                },
                    IntPtr.Zero);
                ClientNatives.CloseDesktop(handle);
            }
            else
            {
                ClientNatives.PostMessage(ClientNatives.GetForegroundWindow(), WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }

            // We need to restart the counter for next screensaver according to
            // https://support.microsoft.com/en-us/kb/140723
            // (this may not be needed since we simulate mouse click afterwards)

            var dummy = IntPtr.Zero;

            // Doesn't really matter if this fails
            ClientNatives.SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1 /* TRUE */, ref dummy, SPIF_SENDWININICHANGE);

        }

        public static string GetForegroundWindowTitle()
        {
            var sbTitle = new StringBuilder(1024);

            ClientNatives.GetWindowText(ClientNatives.GetForegroundWindow(), sbTitle, sbTitle.Capacity);

            return sbTitle.ToString();
        }
    }
}
