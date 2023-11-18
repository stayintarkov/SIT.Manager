using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SIT.Manager.Classes
{
    public class Utils
    {
        public static HttpClient utilsHttpClient = new()
        {
            Timeout = Timeout.InfiniteTimeSpan,
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
        /// Cleans up the EFT directory
        /// </summary>
        /// <returns></returns>
        public void CleanUpEFTDirectory()
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
        /// Extracts a patcher Zip
        /// </summary>
        /// <returns></returns>
        public async static Task ExtractPatcher()
        {
            var window = (Application.Current as App)?.m_window as MainWindow;
            DispatcherQueue mainQueue = window.DispatcherQueue;

            try
            {
                using ZipArchive zip = ZipArchive.Open(App.ManagerConfig.InstallPath + @"\Patcher.zip");
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
                        file.WriteToDirectory(App.ManagerConfig.InstallPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });

                        completed++;
                        progressBar.Report(((float)completed / totalFiles.Count()) * 100);
                        mainQueue.TryEnqueue(() => { window.actionTextBlock.Text = $"Extracting file {completed}/{totalFiles.Count()}"; });
                    }
                }

                mainQueue.TryEnqueue(() =>
                {
                    window.actionPanel.Visibility = Visibility.Collapsed;
                    window.actionProgressRing.Visibility = Visibility.Collapsed;
                });

                window.ShowSimpleNotification("Extract Patcher", "Extraction of patcher was successful.");
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("Error when opening Patcher Archive: " + ex.Message + "\n" + ex);
            }
        }
    }
}
