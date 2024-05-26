using System;
using System.Diagnostics;
using System.IO;

namespace AppFolder {
    public class AppFolderInstaller {
        static readonly string localApplicationData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppFolder");
        static readonly string foldersPath = Path.Combine(localApplicationData, "folders");
        static readonly string iconsPath = Path.Combine(localApplicationData, "icons");

        public static void install() {
            Utils.DownloadFile("https://cdn.discordapp.com/attachments/1009287368311844945/1194162204275113994/8Mlm1g6.png?ex=65f92d37&is=65e6b837&hm=834243967b11cee0a1f313b56211219a60b99ac8c5ea7a91acca9dc6b488b9d5&", iconsPath + "\\base.png");
            Utils.DownloadFile("https://cdn.discordapp.com/attachments/1009287368311844945/1214777137655975976/lkkq74l.py?ex=65fa585f&is=65e7e35f&hm=fcef1dd54cfd1ab40dc3f65c4ce7b217cac623efa9e4b01212c70fee5cd3885a&", Path.Combine(localApplicationData, "main.py"));
            Utils.DownloadFile("https://cdn.discordapp.com/attachments/1009287368311844945/1197489490252537876/0ZrSpu5.txt?ex=65f2d2fe&is=65e05dfe&hm=b7b3122a3d9e364a48249709d7f2a44fa9aa41ede6131af77dea72206590d3bd&", Path.Combine(localApplicationData, "requirements.txt"));
            
            var cmd = new ProcessStartInfo();
            var process = new Process();
            cmd.FileName = @"cmd";
            cmd.WindowStyle = ProcessWindowStyle.Hidden;             // cmd창이 숨겨지도록 하기
            cmd.CreateNoWindow = true;                               // cmd창을 띄우지 안도록 하기
 
            cmd.UseShellExecute = false;
            cmd.RedirectStandardOutput = true;         // cmd창에서 데이터를 가져오기
            cmd.RedirectStandardInput = true;          // cmd창으로 데이터 보내기
            cmd.RedirectStandardError = true;          // cmd창에서 오류 내용 가져오기
 
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();
            process.StandardInput.Write("c:" + Environment.NewLine);
            process.StandardInput.Write("cd " + localApplicationData + Environment.NewLine);
            process.StandardInput.Write("python -m venv .venv" + Environment.NewLine);
            process.StandardInput.Write("call \".venv\\scripts\\activate\"" + Environment.NewLine);
            process.StandardInput.Write("pip install -r requirements.txt" + Environment.NewLine);
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