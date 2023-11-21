using SIT.Manager.Updater.Classes;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Compression;

Console.WriteLine("Ready to download latest version.");
Console.WriteLine("Press any key to start...");
Console.ReadLine();

Process[] processes = Process.GetProcessesByName("SIT.Manager");
if (processes.Length > 0)
{
    Console.WriteLine("An instance of 'SIT.Manager' was found. Would you like to close all instances? Y/N");
    string? response = Console.ReadLine();
    if (response.ToLower() == "y")
	{
		foreach (Process process in processes)
		{
			Console.WriteLine("Killing " + process.ProcessName);
			process.Kill();
			Console.WriteLine();
		}
	}
	else
	{
        Environment.Exit(2);
    }
}

string workingDir = AppDomain.CurrentDomain.BaseDirectory;
HttpClient httpClient = new();

if (!File.Exists(workingDir + @"\SIT.Manager.exe"))
{
	Console.WriteLine("Unable to find 'SIT.Manager.exe' in root directory. Make sure the app is installed correctly.");
	Console.ReadKey();
	Environment.Exit(2);
}

using (var progressBar = new ProgressBar())
{
	Console.WriteLine("Downloading 'SIT.Manager.zip' to " + workingDir);
	try
	{
		Progress<float> progress = new Progress<float>((prog) => { progressBar.Report(prog / 100); });
		using (var file = new FileStream(workingDir + @"\SIT.Manager.zip", FileMode.Create, FileAccess.Write, FileShare.None))
			await HttpClientProgressExtensions.DownloadDataAsync(httpClient, "https://github.com/stayintarkov/SIT.Manager/releases/latest/download/SIT.Manager.zip", file, progress);
	}
	catch (Exception ex)
	{
		Console.WriteLine("Error during download: " + ex.Message);
		Console.WriteLine("Press any key to exit");
		Console.ReadKey();
		Environment.Exit(2);
	}
}

Console.WriteLine("Download complete.");
Console.WriteLine("Creating backup of SIT.Manager");

if (Directory.Exists(workingDir + @"\Backup"))
{
	Directory.Delete(workingDir + @"\Backup", true);	
}

Directory.CreateDirectory(workingDir + @"\Backup");

await Utils.MoveDirectory(workingDir, workingDir + @"\Backup");

Console.WriteLine();
Console.WriteLine("Backup complete. Extracting new version...");
Console.WriteLine();

ZipFile.ExtractToDirectory(workingDir + @"\SIT.Manager.zip", workingDir, false);
File.Delete(workingDir + @"\SIT.Manager.zip");

await Utils.MoveDirectory(workingDir + @"\Release", workingDir);
Directory.Delete(workingDir + @"\Release", true);

Console.WriteLine();
Console.WriteLine(@"Update done. Backup can be found in the '\Backup' folder. Your settings have been saved.");
Console.WriteLine("Press any key to finish...");
Console.ReadKey();

Process.Start("SIT.Manager.exe");

Environment.Exit(0);