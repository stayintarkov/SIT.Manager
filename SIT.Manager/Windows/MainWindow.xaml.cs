using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SIT.Manager.Classes;
using SIT.Manager.Pages;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WinUIEx;
using WinUIEx.Messaging;

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
        public ProgressBar actionProgressBar;
        public ProgressRing actionProgressRing;
        public TextBlock actionTextBlock;

        public MainWindow()
        {
            this.InitializeComponent();

            // Customize Window
            AppWindow.Resize(new(800, 450));
            Title = "SIT Manager";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            WindowManager manager = WindowManager.Get(this);
            manager.MinHeight = 450;
            manager.MaxHeight = 600;
            manager.MinWidth = 800;
            manager.MaxWidth = 1200;

            // Navigate to Play page by default
            NavView.SelectedItem = NavView.MenuItems.FirstOrDefault();
            ContentFrame.Navigate(typeof(PlayPage), null, new SuppressNavigationTransitionInfo());

            // Set up variables to be accessed outside MainWindow
            actionPanel = ActionPanel;
            contentFrame = ContentFrame;
            actionProgressBar = ActionPanelBar;
            actionProgressRing = ActionPanelRing;
            actionTextBlock = ActionPanelText;

            // Create task to prevent the UI thread from freezing on startup?
            Task.Run(() =>
            {
                LookForUpdate();
            });
            
        }

        /// <summary>
        /// Look for an update for SIT.Manager
        /// </summary>
        public async void LookForUpdate()
        {
            string? currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            string latestVersion = await Utils.utilsHttpClient.GetStringAsync(@"https://raw.githubusercontent.com/stayintarkov/SIT.Manager/master/VERSION");
            latestVersion = latestVersion.Trim();

            if (currentVersion != latestVersion)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    UpdateInfoBar.Title = "Update:";
                    UpdateInfoBar.Message = "There is a new update available for SIT.Manager";
                    UpdateInfoBar.Severity = InfoBarSeverity.Informational;

                    UpdateInfoBar.IsOpen = true;

                    await Task.Delay(TimeSpan.FromSeconds(30));

                    UpdateInfoBar.IsOpen = false;
                });
            }

        }

        /// <summary>
        /// Show a simple native toast notification
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="content">The content of the notification</param>
        public void ShowSimpleNotification(string title, string content)
        {
            AppNotification simpleNotification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(content)
                .BuildNotification();

            AppNotificationManager.Default.Show(simpleNotification);
        }

        /// <summary>
        /// Shows the InfoBar of the main window
        /// </summary>
        /// <param name="title">Title of the message</param>
        /// <param name="message">The message to show</param>
        /// <param name="severity">The <see cref="InfoBarSeverity"/> to display</param>
        /// <param name="delay">The delay (in seconds) before removing the InfoBar</param>
        /// <returns></returns>
        public async Task ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int delay = 5)
        {
            MainInfoBar.Title = title;
            MainInfoBar.Message = message;
            MainInfoBar.Severity = severity;

            MainInfoBar.IsOpen = true;

            await Task.Delay(TimeSpan.FromSeconds(delay));

            MainInfoBar.IsOpen = false;
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

                NavigationViewItem settings = (NavigationViewItem)NavView.SettingsItem;
                if (settings.InfoBadge != null)
                {
                    settings.InfoBadge = null;
                }
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
            NavigationViewItem settings = (NavigationViewItem)NavView.SettingsItem;
            FontFamily fontFamily = (FontFamily)Application.Current.Resources["BenderFont"];

            settings.FontFamily = fontFamily;
            if (App.ManagerConfig.InstallPath == null)
            {
                settings.InfoBadge = new()
                {
                    Value = 1
                };
                InstallPathTip.IsOpen = true;
            }
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

        /// <summary>
        /// Handles the Update button on the notification when an update is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string dir = AppContext.BaseDirectory;

            if (File.Exists(dir + @"\SIT.Manager.Updater.exe"))
            {
                Process.Start(dir + @"\SIT.Manager.Updater.exe");
                Application.Current.Exit();
            }
        }
    }
}
