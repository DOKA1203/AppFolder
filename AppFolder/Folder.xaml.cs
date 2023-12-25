using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AppFolder.Utils;

namespace AppFolder
{
    public partial class Folder : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private IntPtr hookId = IntPtr.Zero;
        private HwndSource hwndSource;
        
        private FolderClass folderClass;
        
        public Folder(int id) {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            var pi = CursorPosition.GetCursorPosition();

            folderClass = Util.getFolder(id);
            
            Width = 100 + folderClass.files.Count * 120;
            Height = 150;
            
            Left = Math.Min(pi.X, SystemParameters.WorkArea.Right - Width);
            Top = Math.Min(pi.Y, SystemParameters.WorkArea.Bottom - Height);

            var items = new List<RealFolderIcon>();

            foreach (var folderClassFile in folderClass.files) {
                var rfi = new RealFolderIcon {
                    ImagePath = new BitmapImage(new Uri(folderClassFile.Icon)),
                    Name = folderClassFile.Name,
                    Path = folderClassFile.Path
                };
                items.Add(rfi);
            }

            lvControl.ItemsSource = items;
        }
        
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MessageBox.Show(((Grid)sender).Name);
            var lnkPath = @"C:\Path\To\Your\File.lnk";
            var result = ShellExecute(IntPtr.Zero, "open", lnkPath, "", "", 1);

            if (result > 32){
                Console.WriteLine("성공적으로 실행되었습니다.");
            }
            else{
                Console.WriteLine("실행 중에 오류가 발생했습니다. 오류 코드: " + result);
            }
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

    class RealFolderIcon {
        public string Name { get; set; }
        public BitmapImage ImagePath { get; set; }
        public string Path { get; set; }
    }
}