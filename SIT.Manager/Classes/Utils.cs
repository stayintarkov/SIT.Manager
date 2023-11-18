using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SIT.Manager.Classes
{
    public class Utils
    {
        public static void CheckEFTVersion(string path)
        {
            path = path + @"\EscapeFromTarkov.exe";
            Debug.WriteLine(path);
            if (File.Exists(path))
            {
                string fileVersion = FileVersionInfo.GetVersionInfo(path).ProductVersion;
                fileVersion = Regex.Match(fileVersion, @"[0]{1,}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}\-[0-9]{1,5}").Value.Replace("-", ".");
                App.ManagerConfig.TarkovVersion = fileVersion;
                Loggy.LogToFile("EFT Version is now: " + fileVersion);
            }
            else
            {
                Loggy.LogToFile("CheckEFTVersion: File did not exist at " + path);
            }
        }
    }
}
