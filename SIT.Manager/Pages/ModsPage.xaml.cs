using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModsPage : Page
    {
        public ModsPage()
        {
            this.InitializeComponent();

            if (App.ManagerConfig.AcceptedModsDisclaimer == true)
            {
                DisclaimerGrid.Visibility = Visibility.Collapsed;
                ModGrid.Visibility = Visibility.Visible;
            }

            LoadMasterList();
        }        

        private void LoadMasterList()
        {
            string dir = App.ManagerConfig.InstallPath + @"\SITLauncher\Mods\Extracted\";

            if (!File.Exists(dir + @"MasterList.json"))
            {
                ModsList.Items.Add(new ModInfo()
                {
                    Name = "No mods found"
                });
                ModsList.IsHitTestVisible = false;
                return;
            }

            //if (ModsList.Items.Count > 0)
            //    ModsList.Items.Clear();

            if (ModsList.IsHitTestVisible == false)
                ModsList.IsHitTestVisible = true;

            List<ModInfo> masterList = JsonSerializer.Deserialize<List<ModInfo>>(File.ReadAllText(dir + @"MasterList.json"));

            ModsList.ItemsSource = masterList;
            if (ModsList.Items.Count > 0)
            {
                ModsList.SelectedIndex = 0;

                if (InfoGrid.Visibility == Visibility.Collapsed)
                    InfoGrid.Visibility = Visibility.Visible;
            }
        }

        private async void DownloadModPack()
        {
            try
            {
                DownloadModPackageButton.IsEnabled = false;
                Loggy.LogToFile("DownloadModPack: Starting download of mod package.");

                string dir = App.ManagerConfig.InstallPath + @"\SITLauncher\Mods";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string[] subDirs = Directory.GetDirectories(dir);
                foreach (string subDir in subDirs)
                {
                    Directory.Delete(subDir, true);
                }

                Directory.CreateDirectory(dir + @"\Extracted");

                await Utils.DownloadFile("SIT.Mod.Ports.Collection.zip", dir, "https://github.com/stayintarkov/SIT-Mod-Ports/releases/latest/download/SIT.Mod.Ports.Collection.zip", true);
                Utils.ExtractArchive(dir + @"\SIT.Mod.Ports.Collection.zip", dir + @"\Extracted");
                DownloadModPackageButton.IsEnabled = true;

                LoadMasterList();
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("DownloadModPack:\n" + ex.Message);
                DownloadModPackageButton.IsEnabled = true;
                return;
            }
        }        

        private async void InstallMod(ModInfo mod)
        {
            try
            {
                if (mod.SupportedVersion != App.ManagerConfig.SitVersion)
                {
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        XamlRoot = XamlRoot,
                        Title = "Warning",
                        Content = $"The mod you are trying to install is not compatible with your currently installed version of SIT.\n\nSupported SIT Version: {mod.SupportedVersion}\nInstalled SIT Version: {(string.IsNullOrEmpty(App.ManagerConfig.SitVersion) ? "Unknown" : App.ManagerConfig.SitVersion)}\n\nContinue anyway?",
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = "Yes",
                        CloseButtonText = "No"
                    };

                    ContentDialogResult response = await contentDialog.ShowAsync();

                    if (response != ContentDialogResult.Primary)
                        return;
                }

                InstallButton.IsEnabled = false;
                if (mod == null || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
                    return;

                string installPath = App.ManagerConfig.InstallPath;
                string gamePluginsPath = installPath + @"\BepInEx\plugins\";
                string gameConfigPath = installPath + @"\BepInEx\config\";

                foreach (string pluginFile in mod.PluginFiles)
                {
                    File.Copy(installPath + @"\SITLauncher\Mods\Extracted\plugins\" + pluginFile, gamePluginsPath + pluginFile, true);
                }

                foreach (var configFile in mod.ConfigFiles)
                {
                    File.Copy(installPath + @"\SITLauncher\Mods\Extracted\config\" + configFile, gameConfigPath + configFile, true);
                }

                App.ManagerConfig.InstalledMods.Add(mod.Name);
                App.ManagerConfig.Save();

                Utils.ShowInfoBar("Install Mod", $"{mod.Name} was successfully installed.", InfoBarSeverity.Success);
                UninstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("InstallMod: " + ex.Message);
                InstallButton.IsEnabled = true;
                Utils.ShowInfoBar("Install Mod", $"{mod.Name} failed to install. Check your Launcher.log", InfoBarSeverity.Error);
                return;
            }
        }

        private void UninstallMod(ModInfo mod)
        {
            try
            {
                UninstallButton.IsEnabled = false;

                if (mod == null || string.IsNullOrEmpty(App.ManagerConfig.InstallPath))
                    return;

                string installPath = App.ManagerConfig.InstallPath;
                string gamePluginsPath = installPath + @"\BepInEx\plugins\";
                string gameConfigPath = installPath + @"\BepInEx\config\";

                foreach (string pluginFile in mod.PluginFiles)
                {
                    File.Delete(gamePluginsPath + pluginFile);
                }

                foreach (var configFile in mod.ConfigFiles)
                {
                    File.Delete(gameConfigPath + configFile);
                }

                App.ManagerConfig.InstalledMods.RemoveAll((x) => x == mod.Name);
                App.ManagerConfig.Save();

                Utils.ShowInfoBar("Uninstall Mod", $"{mod.Name} was successfully uninstalled.", InfoBarSeverity.Success);
                InstallButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Loggy.LogToFile("UninstallMod: " + ex.Message);
                UninstallButton.IsEnabled = true;
                Utils.ShowInfoBar("Uninstall Mod", $"{mod.Name} failed to uninstall. Check your Launcher.log", InfoBarSeverity.Error);
                return;
            }
        }

        #region Buttons & Events
        private void DownloadModPackageButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadModPack();
        }
        private void ModsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModInfo selectedMod = (ModInfo)ModsList.SelectedItem;

            if (selectedMod == null)
                return;

            bool isInstalled = App.ManagerConfig.InstalledMods.Contains(selectedMod.Name);

            InstallButton.IsEnabled = !isInstalled;
            UninstallButton.IsEnabled = isInstalled;
        }
        private void IUnderstandButton_Click(object sender, RoutedEventArgs e)
        {
            DisclaimerGrid.Visibility = Visibility.Collapsed;
            ModGrid.Visibility = Visibility.Visible;

            App.ManagerConfig.AcceptedModsDisclaimer = true;
            App.ManagerConfig.Save();
        }
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModsList.SelectedIndex == -1 || ModsList.SelectedItem == null)
                return;

            InstallMod((ModInfo)ModsList.SelectedItem);
        }
        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModsList.SelectedIndex == -1 || ModsList.SelectedItem == null)
                return;

            UninstallMod((ModInfo)ModsList.SelectedItem);
        } 
        #endregion
    }
}
