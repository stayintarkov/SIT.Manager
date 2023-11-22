using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using SIT.Manager.Controls;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using System.Reflection;

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

            VersionHyperlinkButton.Content = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
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
                Utils.CheckSITVersion(eftFolder.Path);

                App.ManagerConfig.Save();
            }
        }

        private async void ChangeAkiServerPath_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            folderPicker.FileTypeFilter.Add("*");

            Window window = new();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder akiServerPath = await folderPicker.PickSingleFolderAsync();
            if (akiServerPath != null)
            {
                App.ManagerConfig.AkiServerPath = akiServerPath.Path;

                App.ManagerConfig.Save();
            }
        }

        private async void ColorChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog colorPickerWindow = new()
            {
                XamlRoot = Content.XamlRoot
            };

            await colorPickerWindow.ShowAsync();

            string pickedColor = colorPickerWindow.SelectedColor;

            if(pickedColor != null)
            {
                App.ManagerConfig.ConsoleFontColor = pickedColor;
                App.ManagerConfig.Save();
            }
        }

        private async void ConsoleFamilyFontChange_Click(object sender, RoutedEventArgs e)
        {
            FontFamilyPickerDialog fontFamilyPickerDialog = new()
            {
                XamlRoot = Content.XamlRoot
            };

            await fontFamilyPickerDialog.ShowAsync();

            string pickedFontFamily = fontFamilyPickerDialog.selectedFontFamily;

            if (pickedFontFamily != null)
            {
                App.ManagerConfig.ConsoleFontFamily = pickedFontFamily;
                App.ManagerConfig.Save();
            }
        }

        private void VersionHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy,
            };

            dataPackage.SetText(VersionHyperlinkButton.Content.ToString());            
            Clipboard.SetContent(dataPackage);
        }
    }
}
