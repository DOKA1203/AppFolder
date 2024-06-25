using System;
using System.IO;
using System.Windows;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace AppFolder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");
        static readonly string iconsPath = Path.Combine(localApplicationData, "icons");

        private void onStartup(object sender, StartupEventArgs e) {

            if (!Directory.Exists(localApplicationData)) {
                Directory.CreateDirectory(localApplicationData);
                Directory.CreateDirectory(foldersPath);
                Directory.CreateDirectory(iconsPath);
            } 

            if (e.Args.Length == 0) {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            } else if (e.Args.Length == 1) {
                if (e.Args[0] == "uninstall") {
                    
                } else if (e.Args[0] == "new") {
                    var lastId = 0;
                    var jsonFiles = Directory.GetFiles(foldersPath, "*.json");
                    foreach (var jsonFile in jsonFiles) {
                        lastId = Math.Max(int.Parse(Path.GetFileNameWithoutExtension(jsonFile)), lastId);
                    }
                    var folder = new Folder(++lastId);
                }
                else {
                    var folderWindow = new FolderWindow(e.Args[0]);
                    folderWindow.Show();
                }
            }
            else {
                var addedApplication = e.Args[1];
                var folderId = e.Args[0];
                var folder = new Folder(int.Parse(folderId));
                var appName = Path.GetFileNameWithoutExtension(addedApplication);
                var pngPath = Path.Combine(iconsPath, folderId, appName, "icon.png");
                var copyPath = Path.Combine(iconsPath, folderId, appName, Path.GetFileName(addedApplication));
                Directory.CreateDirectory(Path.Combine(iconsPath, folderId, appName));
                Utils.BitmapSourceToPngFile(Utils.GetIconFromLink(addedApplication), pngPath);
                File.Copy(addedApplication, copyPath, true);
                
                folder.data.files.Add(new FolderApp {
                    Name = appName,
                    Icon = pngPath,
                    Path = copyPath
                });
                folder.save();
                IconManager.GenerateIcon(folderId);
                
                
                var shell = new WshShell();
                var links = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.lnk");
                foreach (var link in links) {
                    var shortcut = (IWshShortcut)shell.CreateShortcut(link);
                    if (shortcut.Arguments != folderId) {
                        continue;
                    }
                    var icons = Directory.GetFiles(Path.Combine(localApplicationData, "icons", folderId), "*.ico");
                    foreach (var ico in icons) {
                        shortcut.IconLocation = ico;
                        shortcut.Save();
                    }
                    break;
                }
                Utils.KillProcess();
            }
        }
    }
}