using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Manager.Classes
{
    public static class AkiServer
    {
        #region events
        public static event OutputDataReceivedEventHandler? OutputDataReceived;
        public delegate void OutputDataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        public static event StateChangedEventHandler? RunningStateChanged;
        public delegate void StateChangedEventHandler(bool isRunning);
        #endregion

        #region properties

        public static Process? Process;

        public static string ExeName
        {
            get => "Aki.Server.exe";
        }

        public static string FilePath
        {
            get => Path.Combine(App.ManagerConfig.AkiServerPath, ExeName);
        }

        public static string Directory
        {
            get => App.ManagerConfig.AkiServerPath;
        }

        private static bool _isRunning = false;
        public static bool IsRunning 
        {
            get => _isRunning;
        }
        #endregion

        public static bool IsUnhandledInstanceRunning()
        {
            Process[] akiServerProcesses = Process.GetProcessesByName(ExeName.Replace(".exe", ""));
            
            if(akiServerProcesses.Length > 0)
            {
                if (Process == null || Process.HasExited)
                    return true;

                foreach(Process akiServerProcess in akiServerProcesses)
                {
                    if(Process.Id != akiServerProcess.Id)
                        return true;
                }
            }

            return false;
        }

        public static bool Start()
        {            
            if(_isRunning)
                return true;

            Process = new Process();

            Process.StartInfo.FileName = FilePath;
            Process.StartInfo.WorkingDirectory = Directory;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.OutputDataReceived += OutputDataReceivedEvent;
            Process.StartInfo.CreateNoWindow = true;

            Process.Start();
            Process.BeginOutputReadLine();

            UpdateRunningState();

            return _isRunning;
        }

        public static bool Stop()
        {
            if(!_isRunning || Process == null || Process.HasExited)
                return true;

            // this allows to gracefully close a console app.
            Win32.CloseConsoleProgram(Process);

            UpdateRunningState();

            return !_isRunning;
        }

        private static void OutputDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(sender, e);
        }

        private static void UpdateRunningState()
        {
            if (Process == null)
                return;

            bool currentState = !Process.HasExited;

            if (_isRunning != currentState)
            {
                _isRunning = currentState;
                RunningStateChanged?.Invoke(_isRunning);
            }
        }
    }
}
