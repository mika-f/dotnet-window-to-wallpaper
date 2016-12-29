using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace WpfApp1
{
    internal static class NativeMethods
    {
        [DllImport("user32")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32")]
        public static extern IntPtr CreateDC(string pszDriver, string pszDevice, string pszOutput, IntPtr pInitData);

        [DllImport("gdi32")]
        public static extern int DeleteDC(IntPtr hDC);

        [DllImport("user32")]
        public static extern int ReleaceDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32")]
        public static extern int BitBlt(IntPtr hDC, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSource, int nXSource, int nYSource,
                                        ulong dwRaster);
    }
}