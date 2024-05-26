using System;
using System.IO;
using System.Windows;

namespace AppFolder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");

        private int lastId = 0;
        
        public MainWindow() {
            InitializeComponent();
            
            var jsonFiles = Directory.GetFiles(foldersPath, "*.json");
            foreach (var jsonFile in jsonFiles) {
                lastId = Math.Max(int.Parse(Path.GetFileNameWithoutExtension(jsonFile)), lastId);
            }
        }

        private void createFolder(object sender, RoutedEventArgs e) {
            var folder = new Folder(++lastId);
        }
    }
}