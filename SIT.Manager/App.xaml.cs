using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using SIT.Manager.Classes;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static ManagerConfig ManagerConfig { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            ManagerConfig = ManagerConfig.Load();

            Loggy.SetupLogFile();

            AppNotificationManager.Default.NotificationInvoked += ReceivedNotification;
            AppNotificationManager.Default.Register();

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private void ReceivedNotification(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Argument)
                {
                    case "errorInstall=true":
                        Utils.OpenLauncherLog();
                        break;
                }
            }
        }

        private void OnProcessExit(object? sender, EventArgs e)
        {
            AppNotificationManager.Default.RemoveAllAsync();
            AppNotificationManager.Default.Unregister();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // All of below are used to only allow one instance of the app
            // Get the activation args
            var appArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            // Get or register the main instance
            var mainInstance = AppInstance.FindOrRegisterForKey("main");

            // If the main instance isn't this current instance
            if (!mainInstance.IsCurrent)
            {
                // Redirect activation to that instance
                await mainInstance.RedirectActivationToAsync(appArgs);

                // And exit our instance and stop
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            // Otherwise, register for activation redirection
            AppInstance.GetCurrent().Activated += App_Activated;

            m_window = new MainWindow();

            m_window.Activate();
            //m_window.Closed += OnWindowClosed;
        }

        private void App_Activated(object? sender, AppActivationArguments e)
        {
            m_window.DispatcherQueue.TryEnqueue(() => { m_window?.Activate(); });
        }

        void OnWindowClosed(object sender, WindowEventArgs e)
        {
            AppNotificationManager.Default.RemoveAllAsync();
            AppNotificationManager.Default.Unregister();
        }

        internal Window m_window;
    }
}
