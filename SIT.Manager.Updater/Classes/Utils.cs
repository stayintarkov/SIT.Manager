
namespace SIT.Manager.Updater.Classes
{
    internal class Utils
    {        
        public static async Task MoveDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                if (directory.EndsWith("Backup"))
                    continue;

                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                Directory.CreateDirectory(newDirectory);
                await MoveDirectory(directory, newDirectory);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                if (file.EndsWith("SIT.Manager.Updater.exe") || file.EndsWith("SIT.Manager.zip"))
                    continue;

                if (file.EndsWith("ManagerConfig.json"))
                {
                    Console.WriteLine("Moving file " + file.Split(@"\").Last());
                    File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
                    continue;
                }

                Console.WriteLine("Moving file " + file.Split(@"\").Last());
                File.Move(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }
        }
    }
}
