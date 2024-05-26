using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace AppFolder {
    public class Utils {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");

        public static void CreateShortcut(string targetPath,int id, string shortcutName) {
            try {
                // Create WshShell object
                var shell = new WshShell();

                // Create shortcut
                var shortcut = (IWshShortcut)shell.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{shortcutName}.lnk"));
                // Set shortcut properties
                shortcut.TargetPath = targetPath;
                shortcut.Arguments = id.ToString();
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath); // Optional: Set working directory
                shortcut.Description = $"AppFolder id {id}"; // Optional: Set description
                shortcut.IconLocation = targetPath; // Optional: Set icon location
                var icos = Directory.GetFiles(Path.Combine(localApplicationData, "icons", id.ToString()), "*.ico");
                foreach (var ico in icos) {
                    shortcut.IconLocation = ico;
                }
                // shortcut.IconLocation = Path.Combine(localApplicationData, "icons", id.ToString(), "icon.ico");

                shortcut.Save(); // Save the shortcut
            }
            catch (Exception ex) {
                Console.WriteLine($"Error creating shortcut: {ex.Message}");
            }
        }

        public static void DownloadFile(string url, string path) {
            try {
                var webClient = new WebClient();
                webClient.DownloadFile(url, path);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
        
        public static BitmapSource GetIconFromLink(string linkPath) {
            if (!File.Exists(linkPath)) {
                throw new FileNotFoundException("File not found.", linkPath);
            }
            if (linkPath.EndsWith(".url")) {
                return IconToBitmapSource(Icon.ExtractAssociatedIcon(linkPath));
            } else if (linkPath.EndsWith(".lnk")) {
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(linkPath);
                try {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        Icon.ExtractAssociatedIcon(shortcut.TargetPath).Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Exception e) {
                    Console.WriteLine(e);   
                }
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    Icon.ExtractAssociatedIcon(linkPath).Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            throw new FileNotFoundException("File not found.", linkPath);
        }
        public static void BitmapSourceToPngFile(BitmapSource source, string path) {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var stream = File.Create(path)) {
                encoder.Save(stream);
            }
        }
        private static BitmapSource IconToBitmapSource(Icon icon) {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        
        public static void KillProcess() {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.StandardInput.WriteLine($@"taskkill /f /im AppFolder.exe");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
            process.Close();
            try {
                process.Kill();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}