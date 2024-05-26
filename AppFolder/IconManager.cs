using System;

using System.Diagnostics;
using System.IO;

namespace AppFolder
{
    public class IconManager {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");
        static readonly string iconsPath = Path.Combine(localApplicationData, "icons");

        public static void GenerateIcon(string id) {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.StandardInput.WriteLine($@"{localApplicationData}\.venv\Scripts\activate.bat");
            process.StandardInput.WriteLine($@"python {localApplicationData}\main.py {id}");
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