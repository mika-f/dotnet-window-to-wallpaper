using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WpfApp1
{
    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image _bitmap;
        private IntPtr _hwnd;
        private bool _isDraw;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonExitApplicationOnClick(object sender, RoutedEventArgs e)
        {
            if (!_isDraw)
                Application.Current.Shutdown();
            else
                _isDraw = false;
        }

        private void ButtonReflectOnClick(object sender, RoutedEventArgs e)
        {
            if (_isDraw)
                return;
            _isDraw = true;
            _hwnd = new WindowInteropHelper(this).Handle;
            Initialize();
            Task.Run(() => DrawLoop());
        }

        private void ButtonFinishReflectOnClick(object sender, RoutedEventArgs e)
        {
            _isDraw = false;
        }

        // 30 fps でデスクトップに対して描画を行います。
        private void DrawLoop()
        {
            var nextFrame = (double) Environment.TickCount;
            var period = 1000f / 30f;
            while (_isDraw)
            {
                var tick = (double) Environment.TickCount;
                if (tick < nextFrame)
                {
                    if (nextFrame - tick > 1)
                        Thread.Sleep((int) (nextFrame - tick));
                    continue;
                }

                if (Environment.TickCount >= nextFrame + period)
                {
                    nextFrame += period;
                    continue;
                }
                DrawDesktop(_hwnd);
                nextFrame += period;
            }

            // 描画前の状態に復元
            var workerw = IntPtr.Zero;
            User32.EnumWindows((hwnd, lParam) =>
            {
                var shell = User32.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shell != IntPtr.Zero)
                    workerw = User32.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                return true;
            }, IntPtr.Zero);

            var hdc = User32.GetDCEx(workerw, IntPtr.Zero, 0x403);
            using (var graph = Graphics.FromHdc(hdc))
                graph.DrawImage(_bitmap, new PointF(0, 0));
            User32.ReleaseDC(workerw, hdc);
        }

        private void Initialize()
        {
            // WorkerW を起動させる
            var progman = User32.FindWindow("Progman", null);
            User32.SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, 0x0, 1000, out var result);

            // 現在のデスクトップの状態を保存しておく
            var workerw = IntPtr.Zero;
            User32.EnumWindows((hwnd, lParam) =>
            {
                var shell = User32.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shell != IntPtr.Zero)
                    workerw = User32.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                return true;
            }, IntPtr.Zero);

            var hdcSrc = User32.GetDCEx(workerw, IntPtr.Zero, 0x403);
            var hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            var rect = GetRect(workerw);
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;
            var hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            var hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(workerw, hdcSrc);
            _bitmap = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
        }

        /// <summary>
        ///     hWndSrc の内容をデスクトップに描画します。
        /// </summary>
        /// <param name="hWndSrc"></param>
        private void DrawDesktop(IntPtr hWndSrc)
        {
            var workerw = IntPtr.Zero;
            User32.EnumWindows((hwnd, lParam) =>
            {
                var shell = User32.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shell != IntPtr.Zero)
                    workerw = User32.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                return true;
            }, IntPtr.Zero);

            var hWndDest = workerw;
            var hdcSrc = User32.GetDC(_hwnd);
            var hdcDest = User32.GetDCEx(workerw, IntPtr.Zero, 0x403);
            var rect = GetRect(_hwnd);
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;

            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            User32.ReleaseDC(hWndSrc, hdcSrc);
            User32.ReleaseDC(hWndDest, hdcDest);
        }

        private User32.RECT GetRect(IntPtr hWnd)
        {
            var rect = new User32.RECT();
            User32.GetWindowRect(hWnd, ref rect);
            return rect;
        }
    }
}