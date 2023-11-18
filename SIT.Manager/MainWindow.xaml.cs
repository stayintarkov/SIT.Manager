using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SIT.Manager.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public StackPanel actionPanel;
        public Frame contentFrame;

        public MainWindow()
        {
            this.InitializeComponent();

            // Customize Window
            AppWindow.Resize(new(800, 450));
            Title = "SIT Launcher";
            //ExtendsContentIntoTitleBar = true;
            //SetTitleBar(AppTitleBar);

            ContentFrame.Navigate(typeof(PlayPage));

            actionPanel = ActionPanel;
            contentFrame = ContentFrame;
        }

        /// <summary>
        /// Used to navigate the NavView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);
            }
        }

        /// <summary>
        /// Used to set the FontFamily on Settings Button as it has no property in the class. Also adds an InfoBadge to make user aware of the page on first launch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = (NavigationViewItem)NavView.SettingsItem;
            var fontFamily = (FontFamily)App.Current.Resources["BenderFont"];

            settings.FontFamily = fontFamily;
            //if (App.LauncherConfig.InstallPath == null)
            //{
            //    settings.InfoBadge = new()
            //    {
            //        Value = 1
            //    };
            //}
        }

        /// <summary>
        /// Navigates the NavView
        /// </summary>
        /// <param name="item"></param>
        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "Play":
                    ContentFrame.Navigate(typeof(PlayPage));
                    break;
                case "Tools":
                    ContentFrame.Navigate(typeof(ToolsPage));
                    break;
                case "Mods":
                    ContentFrame.Navigate(typeof(ModsPage));
                    break;
            }
        }
    }
}
