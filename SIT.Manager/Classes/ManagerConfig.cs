using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SIT.Manager.Classes
{
    public class ManagerConfig : PropertyChangedBase
    {
        //private ObservableCollection<Server> _servers = new();
        //public ObservableCollection<Server> Servers
        //{
        //    get => _servers;
        //    set => SetField(ref _servers, value);
        //}
        private string _lastServer = "http://127.0.0.1:6969";
        public string LastServer
        {
            get => _lastServer;
            set => SetField(ref _lastServer, value);
        }
        private string _username;
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }
        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }
        private string? _installPath;
        public string InstallPath
        {
            get => _installPath;
            set => SetField(ref _installPath, value);
        }
        private string? _akiServerPath;
        public string AkiServerPath
        {
            get => _akiServerPath;
            set => SetField(ref _akiServerPath, value);
        }
        private bool _rememberLogin = false;
        public bool RememberLogin
        {
            get => _rememberLogin;
            set => SetField(ref _rememberLogin, value);
        }
        private bool _automaticallyInstallSIT = true;
        public bool AutomaticallyInstallSIT
        {
            get => _automaticallyInstallSIT;
            set => SetField(ref _automaticallyInstallSIT, value);
        }
        private bool _installReflectionBinaries = true;
        public bool InstallReflectionBinaries
        {
            get => _installReflectionBinaries;
            set => SetField(ref _installReflectionBinaries, value);
        }
        private bool _closeAfterLaunch = false;
        public bool CloseAfterLaunch
        {
            get => _closeAfterLaunch;
            set => SetField(ref _closeAfterLaunch, value);
        }
        private string? _tarkovVersion;
        public string TarkovVersion
        {
            get => _tarkovVersion;
            set => SetField(ref _tarkovVersion, value);
        }
        private bool _lookForUpdates = true;
        public bool LookForUpdates
        {
            get => _lookForUpdates;
            set => SetField(ref _lookForUpdates, value);
        }
        private Color _consoleFontColor = Colors.LightBlue;
        public Color ConsoleFontColor
        {
            get => _consoleFontColor;
            set => SetField(ref _consoleFontColor, value);
        }

        public static ManagerConfig Load()
        {
            ManagerConfig config = new();

            string currentDir = AppContext.BaseDirectory;

            if (File.Exists(currentDir + @"\ManagerConfig.json"))
                config = JsonSerializer.Deserialize<ManagerConfig>(File.ReadAllText(currentDir + @"\ManagerConfig.json"));

            return config;
        }

        public void Save(bool SaveAccount = false)
        {
            string currentDir = AppContext.BaseDirectory;
            Debug.WriteLine(currentDir);

            if (SaveAccount == false)
            {
                ManagerConfig newLauncherConfig = (ManagerConfig)App.ManagerConfig.MemberwiseClone();
                newLauncherConfig.Username = null;
                newLauncherConfig.Password = null;

                File.WriteAllText(currentDir + "ManagerConfig.json", JsonSerializer.Serialize(newLauncherConfig, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                File.WriteAllText(currentDir + "ManagerConfig.json", JsonSerializer.Serialize(App.ManagerConfig, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }

    public class Server : PropertyChangedBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        private string _address;
        public string Address
        {
            get => _address;
            set => SetField(ref _address, value);
        }
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value,
        [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
