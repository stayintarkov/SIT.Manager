using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using SIT.Manager.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ToolsPage : Page
    {
        public ToolsPage()
        {
            this.InitializeComponent();
        }

        private async void OpenEFTFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (App.ManagerConfig.InstallPath == null)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Config Error",
                    Content = "'Install Path' is not configured. Go to settings to configure the installation path.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
            else
            {
                if (Directory.Exists(App.ManagerConfig.InstallPath))
                    Process.Start("explorer.exe", App.ManagerConfig.InstallPath);
            }
        }

        private async void OpenBepInExFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (App.ManagerConfig.InstallPath == null)
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Config Error",
                    Content = "'Install Path' is not configured. Go to settings to configure the installation path.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }

            string dirPath = App.ManagerConfig.InstallPath + @"\BepInEx\plugins\";
            if (Directory.Exists(dirPath))
                Process.Start("explorer.exe", dirPath);
            else
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Config Error",
                    Content = $"Could not find the given path. Is BepInEx installed?\nPath: {dirPath}",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
        }

        private async void OpenSITConfigButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string path = @"\BepInEx\config\";
            string sitCfg = @"SIT.Core.cfg";

            // Different versions of SIT has different names
            if (!File.Exists(App.ManagerConfig.InstallPath + path + sitCfg))
            {
                sitCfg = "com.sit.core.cfg";
            }

            if (!File.Exists(App.ManagerConfig.InstallPath + path + sitCfg))
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Config Error",
                    Content = $"Could not find '{sitCfg}'. Make sure SIT is installed and that you have started the game once.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }

            Process.Start("explorer.exe", App.ManagerConfig.InstallPath + path + sitCfg);
        }

        private async void OpenEFTLogButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string logPath = @"%userprofile%\AppData\LocalLow\Battlestate Games\EscapeFromTarkov\Player.log";
            logPath = Environment.ExpandEnvironmentVariables(logPath);
            if (File.Exists(logPath))
            {
                Process.Start("explorer.exe", logPath);
            }
            else
            {
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Config Error",
                    Content = "Log file could not be found.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
                return;
            }
        }

        private async void InstallSITButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            GithubRelease? selectedVersion;

            SelectSitVersionDialog selectWindow = new()
            {
                XamlRoot = Content.XamlRoot
            };

            ContentDialogResult result = await selectWindow.ShowAsync();

            selectedVersion = selectWindow.version;

            if (selectedVersion == null || result != ContentDialogResult.Primary)
            {
                return;
            }

            await Task.Run(() => Utils.InstallSIT(selectedVersion));
            ManagerConfig.Save();
        }

        private void OpenLocationEditorButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = App.m_window as MainWindow;

            window.contentFrame.Navigate(typeof(LocationEditor));
        }
        private async void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Battlestate Games", "EscapeFromTarkov");

                // Check if the directory exists.
                if (Directory.Exists(cachePath))
                {
                    // Delete all files within the directory.
                    foreach (string file in Directory.GetFiles(cachePath))
                    {
                        File.Delete(file);
                    }

                    // Delete all subdirectories and their contents.
                    foreach (string subDirectory in Directory.GetDirectories(cachePath))
                    {
                        Directory.Delete(subDirectory, true);
                    }

                    // Optionally, display a success message or perform additional actions.
                    ContentDialog contentDialog = new()
                    {
                        XamlRoot = Content.XamlRoot,
                        Title = "Cache Cleared",
                        Content = "Cache cleared successfully!",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                }
                else
                {
                    // Handle the case where the cache directory does not exist.
                    ContentDialog contentDialog = new()
                    {
                        XamlRoot = Content.XamlRoot,
                        Title = "Cache Clear Error",
                        Content = $"Cache directory not found at: {cachePath}",
                        CloseButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the process.
                ContentDialog contentDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }
    }
}
