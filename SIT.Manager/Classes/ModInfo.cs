using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SIT.Manager.Classes
{
    class ModInfo : PropertyChangedBase
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

        private ObservableCollection<string> _modFiles;
        public ObservableCollection<string> ModFiles
        {
            get => _modFiles;
            set => SetField(ref _modFiles, value);
        }
    }
}
