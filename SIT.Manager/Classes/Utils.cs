using CG.Web.MegaApiClient;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
        /// Downloads a file and shows a progress bar if enabled
        /// </summary>
        /// <param name="fileName">The name of the file to be downloaded.</param>
        /// <param name="filePath">The path (not including the filename) to download to.</param>
        /// <param name="fileUrl">The URL to download from.</param>
        /// <param name="showProgress">If a progress bar should show the status.</param>
        /// <returns></returns>
        public async static Task<bool> DownloadFile(string fileName, string filePath, string fileUrl, bool showProgress = false)
        {
            var window = (Application.Current as App)?.m_window as MainWindow;
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
        public async static Task DownloadAndRunPatcher(string sitVersionTarget = "")
        {
            MainWindow window = (Application.Current as App)?.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            Loggy.LogToFile("Downloading Patcher");
            // todo: proper error message
            if (App.ManagerConfig.TarkovVersion == null)
            {
                Loggy.LogToFile("DownloadPatcher: TarkovVersion is 'null'");
                return;
            }

            string releasesString = await utilsHttpClient.GetStringAsync(@"https://dev.sp-tarkov.com/api/v1/repos/SPT-AKI/Downgrade-Patches/releases");
            List<GiteaRelease>? giteaReleases = JsonSerializer.Deserialize<List<GiteaRelease>>(releasesString);

            // todo: proper error message
            if (giteaReleases == null)
            {
                Loggy.LogToFile("DownloadPatcher: giteaReleases is 'null'");
            }

            List<GiteaRelease> patcherList = new List<GiteaRelease>();
            string tarkovBuild = App.ManagerConfig.TarkovVersion.Split(".").Last();
            string sitBuild = sitVersionTarget.Split(".").Last();
            string tarkovVersionToDowngrade = tarkovBuild != sitBuild ? tarkovBuild : "";

            // find the patcher automatically based on the target SIT version
            if (string.IsNullOrEmpty(tarkovVersionToDowngrade))
            {
                Loggy.LogToFile("DownloadPatcher: tarkovVersionToDowngrade is 'null'");
                return;
            }
            else
            {
                foreach (var release in giteaReleases)
                {
                    if (tarkovVersionToDowngrade == sitBuild)
                    {
                        tarkovVersionToDowngrade = "";
                        break;
                    }

                    var releaseName = release.name;

                    var patcherFrom = releaseName.Split(" to ")[0];
                    var patcherTo = releaseName.Split(" to ")[1];

                    if (patcherFrom == tarkovVersionToDowngrade)
                    {
                        patcherList.Add(release);
                        tarkovVersionToDowngrade = patcherTo;
                    }
                }

                if (!string.IsNullOrEmpty(tarkovVersionToDowngrade))
                {
                    mainQueue.TryEnqueue(async () =>
                    {
                        ContentDialog contentDialog = new()
                        {
                            XamlRoot = window.Content.XamlRoot,
                            Title = "Downgrade Error",
                            Content = "Escape From Tarkov cannot be downgraded to the version required by the selected SIT version.\nMake sure the Escape from Tarkov path configured is compatible with the selected SIT version or use a different SIT version instead.",
                            CloseButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();
                    });

                    return;
                }
            }

            if (patcherList.Count == 0)
                return;

            if (patcherList.Count == 1 && patcherList[0].name.Split(" to ")[0] != App.ManagerConfig.TarkovVersion.Split(".").Last())
            {
                bool warningResult = false;

                mainQueue.TryEnqueue(async () =>
                {
                    ContentDialog contentDialog = new()
                    {
                        XamlRoot = window.Content.XamlRoot,
                        Title = "Warning",
                        Content = $"Your Tarkov version is incorrect for the selected patcher.\nAre you sure you want to continue?\n\nInstalled: {App.ManagerConfig.TarkovVersion.Split(".").Last()}\nRequired: {patcherList[0].name.Split(" to ")[0]}",
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = "Yes",
                        CloseButtonText = "No"
                    };

                    ContentDialogResult contentDialogResult = await contentDialog.ShowAsync();

                    if (contentDialogResult == ContentDialogResult.Primary)
                        warningResult = true;
                });

                if (warningResult == false)
                    return;
            }

            bool success = true;
            string result = "";

            int currentPatcher = 0;
            int currentPatcherCount = patcherList.Count;

            foreach (var patcher in patcherList)
            {
                string mirrorsUrl = patcher.assets.Find(q => q.name == "mirrors.json").browser_download_url;
                string mirrorsString = await utilsHttpClient.GetStringAsync(mirrorsUrl);
                List<Mirrors>? mirrors = JsonSerializer.Deserialize<List<Mirrors>>(mirrorsString);
                string? link = null;

                foreach (Mirrors mirror in mirrors)
                {
                    if (mirror.Link.Contains("gofile.io"))
                    {
                        link = mirror.Link;
                        break;
                    }

                    if (mirror.Link.Contains("mega.nz"))
                    {
                        link = mirror.Link;
                        break;
                    }

                    if (mirror.Link.Contains("dev.sp-tarkov"))
                    {
                        link = mirror.Link;
                        break;
                    }
                }

                if (link == null)
                {
                    Loggy.LogToFile("DownloadPatcher: link is 'null'");
                    return;
                }
                   
                success = await DownloadFile("Patcher.zip", App.ManagerConfig.InstallPath, link, true);

                if (success == false)
                {
                    //todo: proper error message
                    break;
                }

                ExtractArchive(App.ManagerConfig.InstallPath + @"\Patcher.zip", App.ManagerConfig.InstallPath);

                mainQueue.TryEnqueue(() =>
                {
                    window.actionPanel.Visibility = Visibility.Visible;
                    window.actionProgressRing.Visibility = Visibility.Visible;
                    window.actionProgressBar.Visibility = Visibility.Collapsed;
                    window.actionTextBlock.Text = "Copying Patcher files to root directory";
                });

                var patcherDir = Directory.GetDirectories(App.ManagerConfig.InstallPath, "Patcher*").First();

                await CloneDirectory(patcherDir, App.ManagerConfig.InstallPath);
                Directory.Delete(patcherDir, true);

                mainQueue.TryEnqueue(() =>
                {
                    window.actionTextBlock.Text = "Running Patcher";
                });

                result = await RunPatcher();

                if (result != "Patcher was successful." || result == null)
                {
                    break;
                }
            }

            if (result != "Patcher was successful." || result == null)
            {
                mainQueue.TryEnqueue(async () =>
                {
                    window.actionPanel.Visibility = Visibility.Collapsed;
                    window.actionProgressRing.Visibility = Visibility.Collapsed;
                    window.actionProgressBar.Visibility = Visibility.Visible;
                    window.actionTextBlock.Text = "";

                    ContentDialog contentDialog = new()
                    {
                        XamlRoot = window.Content.XamlRoot,
                        Title = "Patcher Error",
                        Content = $"Patcher failed to run:\n{result}\n\nMake sure your folder is clean!",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                });
                return;
            }
            else
            {
                mainQueue.TryEnqueue(async () =>
                {
                    window.actionPanel.Visibility = Visibility.Collapsed;
                    window.actionProgressRing.Visibility = Visibility.Collapsed;
                    window.actionProgressBar.Visibility = Visibility.Visible;
                    window.actionTextBlock.Text = "";

                    ContentDialog contentDialog = new()
                    {
                        XamlRoot = window.Content.XamlRoot,
                        Title = "Patcher Success",
                        Content = result,
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                });
                return;
            }
        }

        /// <summary>
        /// Extracts a Zip archive using SharpCompress
        /// </summary>
        /// <param name="filePath">The file to extract</param>
        /// <param name="destination">The destination to extract to</param>
        /// <returns></returns>
        public static void ExtractArchive(string filePath, string destination)
        {
            var window = (Application.Current as App)?.m_window as MainWindow;
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
            Loggy.LogToFile("Starting Patcher");
            if (!File.Exists(App.ManagerConfig.InstallPath + @"\Patcher.exe"))
                return null;

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
            Loggy.LogToFile("RunPatcher: " + patcherResult);
            return patcherResult;
        }

        /// <summary>
        /// Installs the selected SIT version
        /// </summary>
        /// <param name="selectedVersion">The <see cref="GithubRelease"/> to install</param>
        /// <returns></returns>
        public async static Task InstallSIT(GithubRelease selectedVersion)
        {
            var window = (Application.Current as App)?.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

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

                
                if (App.ManagerConfig.TarkovVersion != selectedVersion.body)
                {
                    await Task.Run(() => DownloadAndRunPatcher(selectedVersion.body));
                    CheckEFTVersion(App.ManagerConfig.InstallPath);
                }                

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

                AppNotification notification = new AppNotificationBuilder()
                    .AddText("Install")
                    .AddText("Installation of SIT was succesful.")
                    .BuildNotification();

                notification.Expiration = DateTime.Now.AddSeconds(5);

                AppNotificationManager.Default.Show(notification);
            }
            catch (Exception ex)
            {
                AppNotificationButton notificationButton = new AppNotificationButton()
                {
                    Content = "Open Log"
                };

                notificationButton.Arguments.Add("errorInstall", "true");

                AppNotification notification = new AppNotificationBuilder()
                    .AddText("Install Error")
                    .AddText("Encountered an error during installation.")
                    .AddButton(notificationButton)
                    .BuildNotification();

                AppNotificationManager.Default.Show(notification);

                Loggy.LogToFile("Install SIT: " + ex.Message + "\n" + ex);

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
    }
}
