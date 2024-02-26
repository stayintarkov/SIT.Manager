using CG.Web.MegaApiClient;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SIT.Manager.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SIT.Manager.Classes
{
    public class Utils
    {
        public static HttpClient utilsHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15),
            DefaultRequestHeaders = {
            { "X-GitHub-Api-Version", "2022-11-28" },
            { "User-Agent", "request" }
        }
        };

        /// <summary>
        /// Checks the installed EFT version
        /// </summary>
        /// <param name="path">The path to check.</param>
        public static void CheckEFTVersion(string path)
        {
            path += @"\EscapeFromTarkov.exe";
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

        /// <summary>
        /// Checks the installed SIT version
        /// </summary>
        /// <param name="path">The path to check.</param>
        public static void CheckSITVersion(string path)
        {
            path += @"\BepInEx\plugins\StayInTarkov.dll";
            if (File.Exists(path))
            {
                string fileVersion = FileVersionInfo.GetVersionInfo(path).ProductVersion;
                fileVersion = Regex.Match(fileVersion, @"[1]{1,}\.[0-9]{1,2}\.[0-9]{1,5}\.[0-9]{1,5}").Value.ToString();
                App.ManagerConfig.SitVersion = fileVersion;
                Loggy.LogToFile("SIT Version is now: " + fileVersion);
            }
            else
            {
                Loggy.LogToFile("CheckSITVersion: File did not exist at " + path);
            }
        }

        /// <summary>
        /// Clones a directory
        /// </summary>
        /// <param name="root">Root path to clone</param>
        /// <param name="dest">Destination path to clone to</param>
        /// <returns></returns>
        public static async Task CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                Directory.CreateDirectory(newDirectory);
                CloneDirectory(directory, newDirectory);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }
        }

        /// <summary>
        /// Downloads a file and shows a progress bar if enabled
        /// </summary>
        /// <param name="fileName">The name of the file to be downloaded.</param>
        /// <param name="filePath">The path (not including the filename) to download to.</param>
        /// <param name="fileUrl">The URL to download from.</param>
        /// <param name="showProgress">If a progress bar should show the status.</param>
        /// <returns></returns>
        public async static Task<bool> DownloadFile(string fileName, string filePath, string fileUrl, bool showProgress = false)
        {
            var window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            if (fileUrl.Contains("mega.nz"))
            {
                try
                {
                    Loggy.LogToFile("Attempting to use Mega API.");

                    MegaApiClient megaApiClient = new MegaApiClient();
                    await megaApiClient.LoginAnonymousAsync();

                    // Todo: Add proper error handling below
                    if (!megaApiClient.IsLoggedIn)
                        return false;

                    Loggy.LogToFile($"Starting download of '{fileName}' from '{fileUrl}'");

                    if (showProgress == true)
                        mainQueue.TryEnqueue(() =>
                        {
                            window.actionPanel.Visibility = Visibility.Visible;
                            window.actionProgressRing.Visibility = Visibility.Visible;
                            window.actionTextBlock.Text = $"Downloading '{fileName}'";
                        });

                    Progress<double> progress = new Progress<double>((prog) => { mainQueue.TryEnqueue(() => { window.actionProgressBar.Value = (int)Math.Floor(prog); }); });

                    Uri fileLink = new(fileUrl);
                    INode fileNode = await megaApiClient.GetNodeFromLinkAsync(fileLink);

                    await megaApiClient.DownloadFileAsync(fileNode, App.ManagerConfig.InstallPath + $@"\{fileName}", progress);

                    if (showProgress == true)
                        mainQueue.TryEnqueue(() =>
                        {
                            window.actionPanel.Visibility = Visibility.Collapsed;
                            window.actionProgressRing.Visibility = Visibility.Collapsed;
                            window.actionTextBlock.Text = "";
                        });

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                Loggy.LogToFile($"Starting download of '{fileName}' from '{fileUrl}'");
                if (showProgress == true)
                    mainQueue.TryEnqueue(() =>
                    {
                        window.actionPanel.Visibility = Visibility.Visible;
                        window.actionProgressRing.Visibility = Visibility.Visible;
                        window.actionTextBlock.Text = $"Downloading '{fileName}'";
                    });

                filePath = filePath + $@"\{fileName}";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                var progress = new Progress<float>((prog) => { mainQueue.TryEnqueue(() => { window.actionProgressBar.Value = (int)Math.Floor(prog); }); });
                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await HttpClientProgressExtensions.DownloadDataAsync(utilsHttpClient, fileUrl, file, progress);

                if (showProgress == true)
                    mainQueue.TryEnqueue(() =>
                    {
                        window.actionPanel.Visibility = Visibility.Collapsed;
                        window.actionProgressRing.Visibility = Visibility.Collapsed;
                        window.actionTextBlock.Text = "";
                    });

                return true;
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("DownloadFile: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Downloads the patcher
        /// </summary>
        /// <param name="sitVersionTarget"></param>
        /// <returns></returns>
        public async static Task<bool> DownloadAndRunPatcher(string url)
        {
            MainWindow window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            Loggy.LogToFile("Downloading Patcher");
            if (App.ManagerConfig.TarkovVersion == null)
            {
                Loggy.LogToFile("DownloadPatcher: TarkovVersion is 'null'");
                return false;
            }

            bool downloadSuccess = await DownloadFile("Patcher.zip", App.ManagerConfig.InstallPath, url, true);
            if (!downloadSuccess)
            {
                Loggy.LogToFile("Failed to download the patcher from the selected mirror.");
                return false;
            }

            string patcherZipPath = App.ManagerConfig.InstallPath + @"\Patcher.zip";

            

            if (File.Exists(patcherZipPath))
            {
                ExtractArchive(patcherZipPath, App.ManagerConfig.InstallPath);
                File.Delete(patcherZipPath);
            }

            var patcherDir = Directory.GetDirectories(App.ManagerConfig.InstallPath, "Patcher*").FirstOrDefault();
            
            if (!string.IsNullOrEmpty(patcherDir))
            {
                await CloneDirectory(patcherDir, App.ManagerConfig.InstallPath);
                Directory.Delete(patcherDir, true);
            }

            string patcherResult = await RunPatcher();
            if (patcherResult != "Patcher was successful.")
            {
                Loggy.LogToFile($"Patcher failed: {patcherResult}");
                return false;
            }

            // If execution reaches this point, it means all necessary patchers succeeded
            Loggy.LogToFile("Patcher completed successfully.");
            return true;
        }

        /// <summary>
        /// Extracts a Zip archive using SharpCompress
        /// </summary>
        /// <param name="filePath">The file to extract</param>
        /// <param name="destination">The destination to extract to</param>
        /// <returns></returns>
        public static void ExtractArchive(string filePath, string destination)
        {
            var window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            try
            {
                using ZipArchive zip = ZipArchive.Open(filePath);
                var files = zip.Entries;

                var totalFiles = files.Where(file => !file.IsDirectory);
                int completed = 0;

                // Show Action Panel
                mainQueue.TryEnqueue(() =>
                {
                    window.actionPanel.Visibility = Visibility.Visible;
                    window.actionProgressRing.Visibility = Visibility.Visible;
                });

                var progress = new Progress<float>((prog) => { mainQueue.TryEnqueue(() => { window.actionProgressBar.Value = (int)Math.Floor(prog); }); });
                IProgress<float> progressBar = progress;

                foreach (var file in files)
                {
                    if (file.IsDirectory == false)
                    {
                        file.WriteToDirectory(destination, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });

                        completed++;
                        progressBar.Report(((float)completed / totalFiles.Count()) * 100);
                        mainQueue.TryEnqueue(() => { window.actionTextBlock.Text = $"Extracting file {file.Key.Split("/").Last()} ({completed}/{totalFiles.Count()})"; });
                    }
                }

                mainQueue.TryEnqueue(() =>
                {
                    window.actionPanel.Visibility = Visibility.Collapsed;
                    window.actionProgressRing.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("ExtractFile: Error when opening Archive: " + ex.Message + "\n" + ex);
            }
        }

        /// <summary>
        /// Runs the downgrade patcher
        /// </summary>
        /// <returns>string with result</returns>
        private async static Task<string> RunPatcher()
        {
            MainWindow window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            Loggy.LogToFile("Starting Patcher");
            if (!File.Exists(App.ManagerConfig.InstallPath + @"\Patcher.exe"))
                return null;

            mainQueue.TryEnqueue(() =>
            {
                window.actionPanel.Visibility = Visibility.Visible;
                window.actionProgressRing.Visibility = Visibility.Visible;
                window.actionTextBlock.Text = $"Running Patcher...";
            });

            Process patcherProcess = new()
            {
                StartInfo = new()
                {
                    FileName = App.ManagerConfig.InstallPath + @"\Patcher.exe",
                    WorkingDirectory = App.ManagerConfig.InstallPath,
                    Arguments = "autoclose"
                },
                EnableRaisingEvents = true
            };
            patcherProcess.Start();
            await patcherProcess.WaitForExitAsync();

            string patcherResult = null;

            switch (patcherProcess.ExitCode)
            {
                case 0:
                    {
                        patcherResult = "Patcher was closed.";
                        break;
                    }
                case 10:
                    {
                        patcherResult = "Patcher was successful.";
                        if (File.Exists(App.ManagerConfig.InstallPath + @"\Patcher.exe"))
                            File.Delete(App.ManagerConfig.InstallPath + @"\Patcher.exe");

                        if (File.Exists(App.ManagerConfig.InstallPath + @"\Patcher.log"))
                            File.Delete(App.ManagerConfig.InstallPath + @"\Patcher.log");

                        if (Directory.Exists(App.ManagerConfig.InstallPath + @"\Aki_Patches"))
                            Directory.Delete(App.ManagerConfig.InstallPath + @"\Aki_Patches", true);

                        break;
                    }
                case 11:
                    {
                        patcherResult = "Could not find 'EscapeFromTarkov.exe'.";
                        break;
                    }
                case 12:
                    {
                        patcherResult = "'Aki_Patches' is missing.";
                        break;
                    }
                case 13:
                    {
                        patcherResult = "Install folder is missing a file.";
                        break;
                    }
                case 14:
                    {
                        patcherResult = "Install folder is missing a folder.";
                        break;
                    }
                case 15:
                    {
                        patcherResult = "Patcher failed.";
                        break;
                    }
                default:
                    {
                        patcherResult = "Unknown error.";
                        break;
                    }

            }

            mainQueue.TryEnqueue(() =>
            {
                window.actionPanel.Visibility = Visibility.Collapsed;
                window.actionProgressRing.Visibility = Visibility.Collapsed;
                window.actionTextBlock.Text = "";
            });

            Loggy.LogToFile("RunPatcher: " + patcherResult);
            return patcherResult;
        }


        /// <summary>
        /// Cleans up the EFT directory
        /// </summary>
        /// <returns></returns>
        public static async Task CleanUpEFTDirectory()
        {
            Loggy.LogToFile("Cleaning up EFT directory...");

            try
            {
                string battlEyeDir = App.ManagerConfig.InstallPath + @"\BattlEye";
                if (Directory.Exists(battlEyeDir))
                {
                    Directory.Delete(battlEyeDir, true);
                }
                string battlEyeExe = App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_BE.exe";
                if (File.Exists(battlEyeExe))
                {
                    File.Delete(battlEyeExe);
                }
                string cacheDir = App.ManagerConfig.InstallPath + @"\cache";
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                }
                string consistencyPath = App.ManagerConfig.InstallPath + @"\ConsistencyInfo";
                if (File.Exists(consistencyPath))
                {
                    File.Delete(consistencyPath);
                }
                string uninstallPath = App.ManagerConfig.InstallPath + @"\Uninstall.exe";
                if (File.Exists(uninstallPath))
                {
                    File.Delete(uninstallPath);
                }
                string logsDirPath = App.ManagerConfig.InstallPath + @"\Logs";
                if (Directory.Exists(logsDirPath))
                {
                    Directory.Delete(logsDirPath);
                }
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("Cleanup: " + ex.Message);
            }

            Loggy.LogToFile("Cleanup done.");
        }


        /// <summary>
        /// Installs the selected SIT version
        /// </summary>
        /// <param name="selectedVersion">The <see cref="GithubRelease"/> to install</param>
        /// <returns></returns>
        public async static Task InstallSIT(GithubRelease selectedVersion)
        {
            var window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            if (string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                Utils.ShowInfoBar("Error", "EFT Path is not set. Configure it in Settings.", InfoBarSeverity.Error);
                return;
            }

            try
            {
                if (selectedVersion == null)
                {
                    Loggy.LogToFile("InstallSIT: selectVersion is 'null'");
                    return;
                }

                if (File.Exists(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_BE.exe"))
                {
                    await CleanUpEFTDirectory();
                }

                if (File.Exists(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\StayInTarkov-Release.zip"))
                    File.Delete(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\StayInTarkov-Release.zip");


                if (!Directory.Exists(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles"))
                    Directory.CreateDirectory(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles");

                if (!Directory.Exists(App.ManagerConfig.InstallPath + @"\SITLauncher\Backup\CoreFiles"))
                    Directory.CreateDirectory(App.ManagerConfig.InstallPath + @"\SITLauncher\Backup\CoreFiles");

                if (!Directory.Exists(App.ManagerConfig.InstallPath + @"\BepInEx\plugins"))
                {
                    await DownloadFile("BepInEx5.zip", App.ManagerConfig.InstallPath + @"\SITLauncher", "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip", true);
                    ExtractArchive(App.ManagerConfig.InstallPath + @"\SITLauncher\BepInEx5.zip", App.ManagerConfig.InstallPath);
                    Directory.CreateDirectory(App.ManagerConfig.InstallPath + @"\BepInEx\plugins");
                }

                //We don't use index as they might be different from version to version
                string releaseZipUrl = selectedVersion.assets.Find(q => q.name == "StayInTarkov-Release.zip").browser_download_url;

                await DownloadFile("StayInTarkov-Release.zip", App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles", releaseZipUrl, true);

                ExtractArchive(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\StayInTarkov-Release.zip", App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\");

                if (File.Exists(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll"))
                    File.Copy(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll", App.ManagerConfig.InstallPath + @"\SITLauncher\Backup\CoreFiles\Assembly-CSharp.dll", true);
                File.Copy(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\StayInTarkov-Release\Assembly-CSharp.dll", App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll", true);

                File.Copy(App.ManagerConfig.InstallPath + @"\SITLauncher\CoreFiles\StayInTarkov-Release\StayInTarkov.dll", App.ManagerConfig.InstallPath + @"\BepInEx\plugins\StayInTarkov.dll", true);

                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SIT.Manager.Resources.Aki.Common.dll"))
                {
                    using (var file = new FileStream(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_Data\Managed\Aki.Common.dll", FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }

                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("SIT.Manager.Resources.Aki.Reflection.dll"))
                {
                    using (var file = new FileStream(App.ManagerConfig.InstallPath + @"\EscapeFromTarkov_Data\Managed\Aki.Reflection.dll", FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }

                // Run on UI thread to prevent System.InvalidCastException, WinUI bug yikes
                mainQueue.TryEnqueue(() =>
                {
                    CheckSITVersion(App.ManagerConfig.InstallPath);
                });

                ShowInfoBar("Install", "Installation of SIT was successful.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBarWithLogButton("Install Error", "Encountered an error during installation.", InfoBarSeverity.Error, 10);

                Loggy.LogToFile("Install SIT: " + ex.Message + "\n" + ex);

                return;
            }
        }



        /// <summary>
        /// Installs the selected SPT Server version
        /// </summary>
        /// <param name="selectedVersion">The <see cref="GithubRelease"/> to install</param>
        /// <returns></returns>
        public async static Task InstallServer(GithubRelease selectedVersion)
        {
            var window = App.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            if (string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
            {
                Utils.ShowInfoBar("Error", "Please configure EFT Path and SPT-AKI Path in Settings.", InfoBarSeverity.Error);
                return;
            }

            try
            {
                if (selectedVersion == null)
                {
                    Loggy.LogToFile("Install Server: selectVersion is 'null'");
                    return;
                }

                // Dynamically find the asset that starts with "SITCoop" and ends with ".zip"
                var releaseAsset = selectedVersion.assets.FirstOrDefault(a => a.name.StartsWith("SITCoop") && a.name.EndsWith(".zip"));
                if (releaseAsset == null)
                {
                    Loggy.LogToFile("No matching release asset found.");
                    return;
                }
                string releaseZipUrl = releaseAsset.browser_download_url;

                string sitServerDirectory = App.ManagerConfig.AkiServerPath;

                // Create the "Server" folder if SPT-Path is not configured.
                if (string.IsNullOrEmpty(sitServerDirectory))
                {
                    // Navigate one level up from InstallPath
                    string baseDirectory = Directory.GetParent(App.ManagerConfig.InstallPath).FullName;

                    // Define the target directory for Server within the parent directory
                    sitServerDirectory = Path.Combine(baseDirectory, "Server");
                }

                // Create SPT-AKI directory (default: Server)
                if(!Directory.Exists(sitServerDirectory))
                {
                    Directory.CreateDirectory(sitServerDirectory);
                }

                // Define the paths for download and extraction based on the Server directory
                string downloadLocation = Path.Combine(sitServerDirectory, releaseAsset.name);
                string extractionPath = sitServerDirectory;

                // Download and extract the file in Server directory
                await DownloadFile(releaseAsset.name, sitServerDirectory, releaseZipUrl, true);
                ExtractArchive(downloadLocation, extractionPath);

                // Remove the downloaded Server after extraction
                File.Delete(downloadLocation);

                // Run on UI thread to prevent System.InvalidCastException, WinUI bug yikes
                mainQueue.TryEnqueue(() =>
                {
                    CheckSITVersion(App.ManagerConfig.InstallPath);
                });

                // Attempt to automatically set the AKI Server Path after successful installation and save it to config
                if (!string.IsNullOrEmpty(sitServerDirectory) && string.IsNullOrEmpty(App.ManagerConfig.AkiServerPath))
                {
                    App.ManagerConfig.AkiServerPath = sitServerDirectory;
                    ManagerConfig.Save();
                    mainQueue.TryEnqueue(() =>
                    {
                        Utils.ShowInfoBar("Config", $"Server installation path set to '{sitServerDirectory}'", InfoBarSeverity.Success);
                    });
                }
                else
                {
                    // Notify user that automatic path detection failed and manual setting is needed
                    Utils.ShowInfoBar("Notice", "Automatic Server path detection failed. Please set it manually.", InfoBarSeverity.Warning);
                }

                ShowInfoBar("Install", "Installation of Server was successful.", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowInfoBarWithLogButton("Install Error", "Encountered an error during installation.", InfoBarSeverity.Error, 10);

                Loggy.LogToFile("Install Server: " + ex.Message + "\n" + ex);

                return;
            }
        }


        /// <summary>
        /// Opens the launcher log
        /// </summary>
        public static void OpenLauncherLog()
        {
            string filePath = AppContext.BaseDirectory + @"Launcher.log";

            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", filePath);
            }
        }

        /// <summary>
        /// Show a simple native toast notification and removes it after 5 seconds
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="content">The content of the notification</param>
        public static async void ShowSimpleNotification(string title, string content)
        {
            AppNotification simpleNotification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(content)
                .BuildNotification();

            AppNotificationManager.Default.Show(simpleNotification);

            await Task.Delay(TimeSpan.FromSeconds(5));

            if (simpleNotification?.Id != null)
            {
                await AppNotificationManager.Default.RemoveByIdAsync(simpleNotification.Id);
            }
        }

        /// <summary>
        /// Shows the InfoBar of the main window
        /// </summary>
        /// <param name="title">Title of the message</param>
        /// <param name="message">The message to show</param>
        /// <param name="severity">The <see cref="InfoBarSeverity"/> to display</param>
        /// <param name="delay">The delay (in seconds) before removing the InfoBar</param>
        /// <returns></returns>
        public static async void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int delay = 5)
        {
            MainWindow window = App.m_window as MainWindow;

            if (window.DispatcherQueue.HasThreadAccess)
            {
                window.DispatcherQueue.TryEnqueue(async () =>
                {
                    InfoBar infoBar = new()
                    {
                        Title = title,
                        Message = message,
                        Severity = severity,
                        IsOpen = true
                    };

                    window.InfoBarStackPanel.Children.Add(infoBar);

                    await Task.Delay(TimeSpan.FromSeconds(delay));

                    window.InfoBarStackPanel.Children.Remove(infoBar);
                });
            }
        }

        /// <summary>
        /// Shows the InfoBar of the main window with an Open Log button
        /// </summary>
        /// <param name="title">Title of the message</param>
        /// <param name="message">The message to show</param>
        /// <param name="severity">The <see cref="InfoBarSeverity"/> to display</param>
        /// <param name="delay">The delay (in seconds) before removing the InfoBar</param>
        /// <returns></returns>
        public static async void ShowInfoBarWithLogButton(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int delay = 5)
        {
            MainWindow window = App.m_window as MainWindow;

            if (window.DispatcherQueue.HasThreadAccess)
            {
                window.DispatcherQueue.TryEnqueue(async () =>
                {
                    Button infoBarButton = new() { Content = "Open Log" };
                    infoBarButton.Click += (e, s) =>
                    {
                        OpenLauncherLog();
                    };

                    InfoBar infoBar = new()
                    {
                        Title = title,
                        Message = message,
                        Severity = severity,
                        IsOpen = true,
                        ActionButton = infoBarButton
                    };

                    window.InfoBarStackPanel.Children.Add(infoBar);

                    await Task.Delay(TimeSpan.FromSeconds(delay));

                    window.InfoBarStackPanel.Children.Remove(infoBar);
                });
            }
        }
    }
}
