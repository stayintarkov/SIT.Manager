using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Windows.AppNotifications;
using SIT.Manager.Classes;

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

            AppNotificationManager.Default.Register();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            m_window.Activate();
        }

        internal Window m_window;
    }
}
