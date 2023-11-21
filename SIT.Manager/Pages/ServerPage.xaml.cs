using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SIT.Manager.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// ServerPage for handling SPT-AKI Server execution and console output.
    /// </summary>
    public sealed partial class ServerPage : Page
    {
        MainWindow? window = (Application.Current as App)?.m_window as MainWindow;

        public ServerPage()
        {
            this.InitializeComponent();

            AkiServer.OutputDataReceived += AkiServer_OutputDataReceived;
            AkiServer.RunningStateChanged += AkiServer_RunningStateChanged;

            Loaded += ServerPage_Loaded;
        }

        private void ServerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ConsoleLog.Foreground = new SolidColorBrush(App.ManagerConfig.ConsoleFontColor);
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AkiServer.IsRunning)
            {
                if (AkiServer.IsUnhandledInstanceRunning())
                {
                    AddConsole("SPT-AKI is currently running. Please close any running instance of SPT-AKI.");

                    return;
                }

                AddConsole("Starting server...");
                
                try
                {
                    AkiServer.Start();
                }
                catch(Exception ex)
                {
                    AddConsole(ex.Message);
                }
            }
            else
            {
                AddConsole("Stopping server...");

                try
                {
                    AkiServer.Stop();
                }
                catch(Exception ex)
                {
                    AddConsole(ex.Message);
                }
            }
        }

        private void AddConsole(string text)
        {
            if(text == null)
                return;

            Paragraph paragraph = new();
            Run run = new();

            //[32m, [2J, [0;0f,
            run.Text = Regex.Replace(text, @"(\[\d{1,2}m)|(\[\d{1}[a-zA-Z])|(\[\d{1};\d{1}[a-zA-Z])", "");

            paragraph.Inlines.Add(run);
            ConsoleLog.Blocks.Add(paragraph);
        }

        private void AkiServer_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            window.DispatcherQueue.TryEnqueue(() =>
            {
                AddConsole(e.Data);
            }); 
        }

        private void AkiServer_RunningStateChanged(bool isRunning)
        {
            if (isRunning)
            {
                AddConsole("Server started!");
                StartServerButtonSymbolIcon.Symbol = Symbol.Stop;
                StartServerButtonTextBlock.Text = "Stop Server";
            }
            else
            {
                AddConsole("Server stopped!");
                StartServerButtonSymbolIcon.Symbol = Symbol.Play;
                StartServerButtonTextBlock.Text = "Start Server";
            }
        }

        private void ConsoleLog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConsoleLogScroller.ScrollToVerticalOffset(ConsoleLogScroller.ScrollableHeight);
        }
    }
}
