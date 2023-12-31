﻿// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using AppFolder.Services;
using AppFolder.Utils;
using AppFolder.ViewModels.Pages;
using AppFolder.ViewModels.Windows;
using AppFolder.Views.Pages;
using AppFolder.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppFolder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<DataPage>();
                services.AddSingleton<DataViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e) {
            // Util.ConvertPngToIco("C:\\Users\\doka\\AppData\\Local\\AppFolder\\base.png", "C:\\Users\\doka\\AppData\\Local\\AppFolder\\icons\\icon.ico");
            
            ParseArgs(e.Args);
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
        
        private static async void ParseArgs(string[] args) {
            var localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");

            var config = Util.getConfig();

            var foldersPath = Path.Combine(localApplicationData, "folders");

            if (args.Length == 0) {
                await _host.StartAsync();
                return;
            }
            else if (args.Length == 1) {
                var folderJson = Path.Combine(foldersPath, $"{args[0]}.json");

                if (File.Exists(folderJson)) {
                    var jsonString = File.ReadAllText(folderJson);
                    var f = JsonSerializer.Deserialize<FolderClass>(jsonString);
                    var folder = new Folder(int.Parse(args[0]));
                    folder.Show();
                }
                return;
            }
            else if (args.Length == 2) {
                var id = args[0];
                var plus = args[1];
                // Do something with 'id' and 'plus'

                //Util.BitmapSourceToPngFile(Util.GetIconFromLnk(plus));
                Directory.CreateDirectory(Path.Combine(localApplicationData, "icons", id,  Path.GetFileNameWithoutExtension(plus)));
                Util.BitmapSourceToPngFile(Util.GetIconFromLink(plus), Path.Combine(localApplicationData, "icons", id , Path.GetFileNameWithoutExtension(plus) , "icon.png"));
                File.Copy(plus, Path.Combine(localApplicationData, "icons", id,  Path.GetFileNameWithoutExtension(plus), Path.GetFileName(plus)), true);
                var folder = Util.getFolder(int.Parse(id));
                folder.files.Add(new FolderIcon {
                    Name = Path.GetFileNameWithoutExtension(plus),
                    Icon = Path.Combine(localApplicationData, "icons", id , Path.GetFileNameWithoutExtension(plus) , "icon.png"),
                    Path = Path.Combine(localApplicationData, "icons", id , Path.GetFileNameWithoutExtension(plus) , Path.GetFileName(plus)),
                });
                Util.saveFolder(folder);
            }
            Process.GetCurrentProcess().Kill();
        }
    }
    public class FolderClass {
        public int id { get; set; }
        public string name { get; set; }
        public List<FolderIcon> files { get; set; }
    }
    public class Config
    {
        public int lastId { get; set; }
    }
    public class FolderIcon {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
    }
}
