using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Everything.Utilities;

namespace Everything.Helper
{
    public static class ScreenHelper
    {
        private const int SRCCOPY = 0x00CC0020;

        public static Bitmap CaptureScreen(int screenNumber)
        {
            var bounds = GetBounds(screenNumber);
            var screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage(screen))
            {
                var destDeviceContext = g.GetHdc();
                var srcDeviceContext = ClientNatives.CreateDC("DISPLAY", null, null, IntPtr.Zero);

                ClientNatives.BitBlt(destDeviceContext, 0, 0, bounds.Width, bounds.Height, srcDeviceContext, bounds.X,
                    bounds.Y, SRCCOPY);

                ClientNatives.DeleteDC(srcDeviceContext);
                g.ReleaseHdc(destDeviceContext);
            }

            return screen;
        }

        public static Rectangle GetBounds(int screenNumber)
        {
            return Screen.AllScreens[screenNumber].Bounds;
        }
    }
}
