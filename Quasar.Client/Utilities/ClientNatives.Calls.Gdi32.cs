using System;
using System.Runtime.InteropServices;

namespace Everything.Utilities
{
    internal static partial class ClientNatives
    {
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
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool BitBltFunc(
            [In] IntPtr hdc,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            [In] IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            int dwRop);
        internal static bool BitBlt(
            IntPtr hdc,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            int dwRop) => Lookup<BitBltFunc>("gdi32.dll", "BitBlt")(hdc, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);

        internal delegate IntPtr CreateDCFunc(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDriver,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszOutput,
            IntPtr lpInitData);
        internal static IntPtr CreateDC(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDriver,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszOutput,
            IntPtr lpInitData) => Lookup<CreateDCFunc>("gdi32.dll", "CreateDCW")(lpszDriver, lpszDevice, lpszOutput, lpInitData);

        internal delegate bool DeleteDCFunc([In] IntPtr hdc);
        internal static bool DeleteDC(IntPtr hdc) => Lookup<DeleteDCFunc>("gdi32.dll", "DeleteDC")(hdc);
    }
}
