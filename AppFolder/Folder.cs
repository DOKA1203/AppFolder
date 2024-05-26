using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using AppFolder;

namespace AppFolder {
    public class FolderData {
        public int id { get; set; }
        public string name { get; set; }
        public List<FolderApp> files { get; set; }
    }
    public class FolderApp {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
    }
    public class Folder {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");
        static readonly string iconsPath = Path.Combine(localApplicationData, "icons");

        public FolderData data;
        
        public Folder(int id) {
            var folderJson = Path.Combine(foldersPath, $"{id}.json");
            if (File.Exists(folderJson)) {
                var jsonString = File.ReadAllText(folderJson);
                data = JsonSerializer.Deserialize<FolderData>(jsonString);
            } else {
                data = new FolderData {
                    id = id,
                    name = $"New Folder {id}",
                    files = new List<FolderApp>()
                };
                this.save();
                Directory.CreateDirectory(Path.Combine(iconsPath, id.ToString()));
                IconManager.GenerateIcon(id.ToString());
                Utils.CreateShortcut("D:\\DokaLab\\AppFolder\\AppFolder\\bin\\Debug\\AppFolder.exe", id, "새 앱폴더 " + id);
            }
        }
        public void save() {
            var jsonString = JsonSerializer.Serialize(data);
            var folderJson = Path.Combine(foldersPath, $"{data.id}.json");
            File.WriteAllText(folderJson, jsonString);
        }
    }
}