using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AppFolder
{
    public partial class Folder : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private IntPtr hookId = IntPtr.Zero;
        private HwndSource hwndSource;

        public Folder() {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            Point pi = CursorPosition.GetCursorPosition();
            
            Left = Math.Min(pi.X, SystemParameters.WorkArea.Right - Width);
            Top = Math.Min(pi.Y, SystemParameters.WorkArea.Bottom - Height);
            this.Show();
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (hwndSource != null) {
                hookId = SetHook();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            UnhookWindowsHookEx(hookId);
        }

        private IntPtr SetHook() {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            using (var module = process.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, HookCallback, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (int)wParam == WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                // 클릭된 위치가 창 안에 속하지 않으면 창을 닫습니다.
                if (!IsPointInsideWindow(hookStruct.pt))
                {
                    this.Dispatcher.Invoke(() => this.Close());
                }
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private bool IsPointInsideWindow(POINT pt)
        {
            RECT windowRect;
            GetWindowRect(hwndSource.Handle, out windowRect);

            return pt.x >= windowRect.Left && pt.x <= windowRect.Right && pt.y >= windowRect.Top && pt.y <= windowRect.Bottom;
        }

        #region Native Methods

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion
    }

    internal static class CursorPosition {
        [StructLayout(LayoutKind.Sequential)]
        public struct PointInter {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out PointInter lpPoint);

        // For your convenience
        public static Point GetCursorPosition() {
            PointInter lpPoint;
            GetCursorPos(out lpPoint);
            return (Point)lpPoint;
        }
    }
}