using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace AppFolder {
    public partial class FolderWindow : Window {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private IntPtr hookId = IntPtr.Zero;
        private HwndSource hwndSource;
        
        Folder folder;
        public FolderWindow(string id) {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            
            folder = new Folder(int.Parse(id));
            
            var pi = CursorPosition.GetCursorPosition();
            Width = folder.data.files.Count * 80 + 20;
            Height = 100;
            
            Left = Math.Min(pi.X, SystemParameters.WorkArea.Right - Width);
            Top = Math.Min(pi.Y, SystemParameters.WorkArea.Bottom - Height);

            
            Apps.ItemsSource = folder.data.files;
        }
        
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var lnkPath = ((Grid)sender).Tag.ToString();
            ShellExecute(IntPtr.Zero, "open", lnkPath, "", "", 1);
            
            Close();
        }
        [DllImport("Shell32.dll")]
        private static extern int ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        private HookProc _hookProcDelegate;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (hwndSource != null) {
                _hookProcDelegate = HookCallback;
                hookId = SetHook();
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            UnhookWindowsHookEx(hookId);
        }
        private IntPtr SetHook() {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule module = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, _hookProcDelegate, GetModuleHandle(module.ModuleName), 0);
            }
        }
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && (int)wParam == WM_LBUTTONDOWN) {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                // 클릭된 위치가 창 안에 속하지 않으면 창을 닫습니다.
                if (!IsPointInsideWindow(hookStruct.pt)) {
                    Dispatcher.Invoke(() => Close());
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
        private bool IsPointInsideWindow(POINT pt) {
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
        private struct POINT {
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
        private static extern bool GetCursorPos(out PointInter lpPoint);
        public static Point GetCursorPosition() {
            PointInter lpPoint;
            GetCursorPos(out lpPoint);
            return (Point)lpPoint;
        }
    }
}