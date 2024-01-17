using SIT.Manager.Updater.Classes;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Compression;

const string SITMANAGER_PROC_NAME = "SIT.Manager.exe";
const string SITMANAGER_RELEASE_URI = @"https://github.com/stayintarkov/SIT.Manager/releases/latest/download/SIT.Manager.zip";

Console.WriteLine("Ready to download latest version.");
Console.WriteLine("Press any key to start...");
Console.ReadKey();

Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(SITMANAGER_PROC_NAME));
if (processes.Length > 0)
{
    Console.WriteLine("An instance of 'SIT.Manager' was found. Would you like to close all instances? Y/N");
    string? response = Console.ReadLine();
    if (string.Equals(response, "y"))
	{
		foreach (Process process in processes)
		{
			Console.WriteLine("Killing {0} with PID {1}\n", process.ProcessName, process.Id);
			bool clsMsgSent = process.CloseMainWindow();
			if (clsMsgSent)
				process.WaitForExit();
			else
				process.Kill();
		}
	}
	else
	{
        Environment.Exit(1);
    }
}

string workingDir = AppDomain.CurrentDomain.BaseDirectory;
HttpClient httpClient = new();

if (!File.Exists(Path.Combine(workingDir, SITMANAGER_PROC_NAME)))
{
	Console.WriteLine("Unable to find '{0}' in root directory. Make sure the app is installed correctly.", SITMANAGER_PROC_NAME);
	Console.ReadKey();
	Environment.Exit(2);
}

string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
Directory.CreateDirectory(tempPath);
string zipName = Path.GetFileName(SITMANAGER_RELEASE_URI);
string zipPath = Path.Combine(tempPath, zipName);
using (var progressBar = new ProgressBar())
{
    Console.WriteLine("Downloading '{0}' to '{1}'", zipName, zipPath);
    try
	{
        Progress<float> progress = new(prog => progressBar.Report(prog / 100));
        using FileStream fs = new(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await HttpClientProgressExtensions.DownloadDataAsync(httpClient, SITMANAGER_RELEASE_URI, fs, progress);
    }
	catch (Exception ex)
	{
		Console.WriteLine("Error during download: {0}", ex.Message);
		Console.WriteLine("Press any key to exit");
		Console.ReadKey();
		Environment.Exit(3);
	}
}

Console.WriteLine("Download complete.");
Console.WriteLine("Creating backup of SIT.Manager");

string backupPath = Path.Combine(workingDir, "Backup");
if (Directory.Exists(backupPath))
	Directory.Delete(backupPath, true);

Directory.CreateDirectory(backupPath);
await Utils.MoveDirectory(workingDir, backupPath);

Console.WriteLine("\nBackup complete. Extracting new version...\n");

ZipFile.ExtractToDirectory(zipPath, tempPath, false);

string releasePath = Path.Combine(tempPath, "Release");
await Utils.MoveDirectory(releasePath, workingDir);

Directory.Delete(tempPath, true);

Console.WriteLine("\nUpdate done. Backup can be found in the '\\Backup' folder. Your settings have been saved.");
Console.WriteLine("Press any key to finish...");
Console.ReadKey();

Process.Start(SITMANAGER_PROC_NAME);

Environment.Exit(0);