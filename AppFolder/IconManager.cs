using System;

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace AppFolder {
    public class IconManager {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");
        static readonly string iconsPath = Path.Combine(localApplicationData, "icons");

        public static void GenerateIcon(string id) {
            ShellExecute(IntPtr.Zero, "open", @"C:\Program Files (x86)\AppFolder\main.exe", id, "", 1);
            Thread.Sleep(3000);
        }

        [DllImport("Shell32.dll")]
        private static extern int ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
    }
}