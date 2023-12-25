using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace AppFolder.Utils;

public class Util
{
    public static string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
    public static string foldersPath = Path.Combine(localApplicationData, "folders");
    public static string iconsPath = Path.Combine(localApplicationData, "icons");
    public static string linksPath = Path.Combine(localApplicationData, "links");
    public static Config getConfig() {
        if (!Directory.Exists(localApplicationData)) {
            Directory.CreateDirectory(localApplicationData);
            Directory.CreateDirectory(foldersPath);
            Directory.CreateDirectory(iconsPath);
            Directory.CreateDirectory(linksPath);
        }
        var filePath = localApplicationData + "\\config.json";
        if (!File.Exists(filePath)) {
            Config c = new Config {
                lastId = 1,
            };
            saveConfig(c);
            return c;
        }
        var jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Config>(jsonString);
    }
    public static void saveConfig(Config config) {
        var jsonString = JsonSerializer.Serialize(config);

        // JSON 문자열을 파일에 저장
        var filePath = localApplicationData + "\\config.json";
        File.WriteAllText(filePath, jsonString);

        Console.WriteLine("JSON 데이터가 파일에 저장되었습니다.");
    }
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
            shortcut.Description = "AppFolder"; // Optional: Set description
            shortcut.IconLocation = targetPath; // Optional: Set icon location
            shortcut.IconLocation = Path.Combine(localApplicationData, "icons", "test.ico");

            shortcut.Save(); // Save the shortcut
        }
        catch (Exception ex) {
            Console.WriteLine($"Error creating shortcut: {ex.Message}");
        }
    }
    public static void ConvertPngToIco(string pngFilePath, string icoFilePath) {
        if (!File.Exists(pngFilePath)) {
            throw new FileNotFoundException("File not found.", pngFilePath);
        }
        using (var pngStream = new FileStream(pngFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            using (var icoStream = new FileStream(icoFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)) {
                ConvertPngToIco(pngStream, icoStream);
            }
        }
    }
    private static void ConvertPngToIco(Stream pngStream, Stream icoStream) {
        var pngImage = new BitmapImage();
        pngImage.BeginInit();
        pngImage.StreamSource = pngStream;
        pngImage.EndInit();

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(pngImage));

        encoder.Save(icoStream);
    }
    private static BitmapSource IconToBitmapSource(Icon icon) {
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
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
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                Icon.ExtractAssociatedIcon(shortcut.TargetPath).Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        throw new FileNotFoundException("File not found.", linkPath);
    }
    public static FolderClass getFolder(int id) {
        var filePath = Path.Combine(localApplicationData, "folders", $"{id}.json");
        if (!File.Exists(filePath)) {
            return null;
        }
        var jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<FolderClass>(jsonString);
    }
    public static void saveFolder(FolderClass folderClass) {
        var jsonString = JsonSerializer.Serialize(folderClass);
        var filePath = Path.Combine(localApplicationData, "folders", $"{folderClass.id}.json");
        File.WriteAllText(filePath, jsonString);
        Console.WriteLine("JSON 데이터가 파일에 저장되었습니다.");
    } 
    public static void BitmapSourceToPngFile(BitmapSource source, String path) {
        //MessageBox.Show("test");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));
        using (var stream = File.Create(path)) {
            encoder.Save(stream);
        }
    } 
}