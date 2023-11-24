// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.IO;
using System.Text.Json;
using AppFolder.Utils;

namespace AppFolder.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _counter = 0;

        [RelayCommand]
        private void OnCounterIncrement()
        {
            var config = Util.getConfig();
            var lastId = config.lastId++;
            string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
            if (!Directory.Exists(localApplicationData))
            {
                Directory.CreateDirectory(localApplicationData);
            }
            string foldersPath = localApplicationData + "\\folders";
            if (!Directory.Exists(foldersPath))
            {
                Directory.CreateDirectory(foldersPath);
            }
            
            var folder = new FolderClass
            {
                id = lastId + 1,
                name = "",
                files = Array.Empty<string>()
            };

            // 객체를 JSON 문자열로 직렬화
            string jsonString = JsonSerializer.Serialize(folder);

            // JSON 문자열을 파일에 저장
            string filePath = foldersPath + $"\\{folder.id}.json";
            File.WriteAllText(filePath, jsonString);

            Console.WriteLine("JSON 데이터가 파일에 저장되었습니다.");
            Util.saveConfig(config);
            
            Util.CreateShortcut($"D:\\DokaLab\\AppFolder\\AppFolder\\bin\\Debug\\net7.0-windows\\AppFolder.exe",folder.id, $"폴더 {folder.id.ToString()}");
        }
    }
}
