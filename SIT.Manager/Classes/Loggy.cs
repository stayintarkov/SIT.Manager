using System;
using System.IO;

namespace SIT.Manager.Classes
{
    public static class Loggy
    {
        public static void SetupLogFile()
        {
            string rootPath = AppContext.BaseDirectory;

            if (File.Exists(rootPath + @"\Launcher.log"))
                File.Move(rootPath + @"\Launcher.log", rootPath + @"\Launcher.log.BAK", true);
            FileStream file = File.Create(rootPath + @"\Launcher.log");
            file.Close();
        }

        public static async void LogToFile(string line)
        {
            string rootPath = AppContext.BaseDirectory;
            string filePath = rootPath + @"\Launcher.log";

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", nameof(filePath));

            using (var file = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Write))
            using (var writer = new StreamWriter(file))
            {
                await writer.WriteLineAsync(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]") + " " + line);
                await writer.FlushAsync();
            }
        }
    }
}
