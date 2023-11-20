using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            DataContext = App.ManagerConfig;
        }

        private async void ChangeInstallButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            Window window = new();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder eftFolder = await folderPicker.PickSingleFolderAsync();
            if (eftFolder != null)
            {
                App.ManagerConfig.InstallPath = eftFolder.Path;

                Utils.CheckEFTVersion(eftFolder.Path);

                App.ManagerConfig.Save();

                var mainWindow = (Application.Current as App)?.m_window as MainWindow;
                DispatcherQueue mainQueue = mainWindow.DispatcherQueue;

                mainWindow.ShowInfoBar("Settings:", $"Install path set to {eftFolder.Path}");
            }
        }
    }
}
