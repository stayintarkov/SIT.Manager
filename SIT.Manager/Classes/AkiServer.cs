using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SIT.Manager.Classes
{
    public static class AkiServer
    {
        #region events
        public static event OutputDataReceivedEventHandler? OutputDataReceived;
        public delegate void OutputDataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        public static event StateChangedEventHandler? RunningStateChanged;
        public delegate void StateChangedEventHandler(RunningState runningState);
        #endregion

        #region fields

        public enum RunningState
        {
            NOT_RUNNING,
            RUNNING,
            STOPPED_UNEXPECTEDLY
        }

        public static string[] Editions;

        public static Process? Process;

        public static string ExeName
        {
            get => "Aki.Server.exe";
        }

        public static string FilePath
        {
            get => App.ManagerConfig.AkiServerPath != null ? Path.Combine(App.ManagerConfig.AkiServerPath, ExeName) : "";
        }

        public static string Directory
        {
            get => App.ManagerConfig.AkiServerPath != null ? App.ManagerConfig.AkiServerPath : "";
        }

        private static RunningState _state = RunningState.NOT_RUNNING;
        public static RunningState State
        {
            get => _state;
        }

        private static bool stopRequest = false;

        #endregion

        public static void Start()
        {
            if (_state == RunningState.RUNNING)
                return;

            Process = new Process();

            Process.StartInfo.FileName = FilePath;
            Process.StartInfo.WorkingDirectory = Directory;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.EnableRaisingEvents = true;
            Process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => OutputDataReceivedEvent(sender, e));
            Process.Exited += new EventHandler((sender, e) => ExitedEvent(sender, e));

            Process.Start();
            Process.BeginOutputReadLine();

            _state = RunningState.RUNNING;

            RunningStateChanged?.Invoke(_state);
        }

        public static void Stop()
        {
            if (_state == RunningState.NOT_RUNNING || Process == null || Process.HasExited)
                return;

            stopRequest = true;

            // this allows to gracefully close a console app.
            Win32.CloseConsoleProgram(Process);
        }

        public static bool IsUnhandledInstanceRunning()
        {
            Process[] akiServerProcesses = Process.GetProcessesByName(ExeName.Replace(".exe", ""));

            if (akiServerProcesses.Length > 0)
            {
                if (Process == null || Process.HasExited)
                    return true;

                foreach (Process akiServerProcess in akiServerProcesses)
                {
                    if (Process.Id != akiServerProcess.Id)
                        return true;
                }
            }

            return false;
        }

        private static void OutputDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(sender, e);
        }

        private static void ExitedEvent(object? sender, EventArgs e)
        {
            if (_state == RunningState.RUNNING && !stopRequest)
            {
                _state = RunningState.STOPPED_UNEXPECTEDLY;
            }
            else
            {
                _state = RunningState.NOT_RUNNING;
            }

            stopRequest = false;
            RunningStateChanged?.Invoke(_state);
        }
    }
}
