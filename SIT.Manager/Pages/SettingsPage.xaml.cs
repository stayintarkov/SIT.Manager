using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using SIT.Manager.Classes;
using System.Diagnostics;

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

            Window window = new();
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder eftFolder = await folderPicker.PickSingleFolderAsync();
            if (eftFolder != null)
            {
                App.ManagerConfig.InstallPath = eftFolder.Path;              

                Utils.CheckEFTVersion(eftFolder.Path);

                App.ManagerConfig.Save();
            }
        }
    }
}
