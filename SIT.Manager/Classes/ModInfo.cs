using System.Collections.Generic;

namespace SIT.Manager.Classes
{
    public class ModInfo : PropertyChangedBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);

        }

        private string _author;
        public string Author
        {
            get => _author;
            set => SetField(ref _author, value);
        }

        private string _supportedVersion;
        public string SupportedVersion
        {
            get => _supportedVersion;
            set => SetField(ref _supportedVersion, value);
        }

        private string _modVersion;
        public string ModVersion
        {
            get => _modVersion;
            set => SetField(ref _modVersion, value);
        }

        private string _portVersion;
        public string PortVersion
        {
            get => _portVersion;
            set => SetField(ref _portVersion, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        private string _modUrl;
        public string ModUrl
        {
            get => _modUrl;
            set => SetField(ref _modUrl, value);
        }

        private bool _requiresFiles;
        public bool RequiresFiles
        {
            get => _requiresFiles;
            set => SetField(ref _requiresFiles, value);
        }

        private List<string> _pluginFiles = new();
        public List<string> PluginFiles
        {
            get => _pluginFiles;
            set => SetField(ref _pluginFiles, value);
        }
        private List<string> _configFiles = new();
        public List<string> ConfigFiles
        {
            get => _configFiles;
            set => SetField(ref _configFiles, value);
        }
    }
}
