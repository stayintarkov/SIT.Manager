using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModsPage : Page
    {
        public ModsPage()
        {
            this.InitializeComponent();

            ObservableCollection<ModInfo> mods = new();

            mods.Add(new ModInfo()
            {
                Name = "SAIN",
                Author = "Solarint & DrakiaXYZ",
                Description = "AI Brain overhaul, making them very cool and dope.",
                ModUrl = "https://hub.sp-tarkov.com/files/file/1062-sain-2-0-solarint-s-ai-modifications-full-ai-combat-system-replacement/",
                ModVersion = "2.0",
                SupportedVersion = "1.9.8713.31794",
                RequiresFiles = true
            });

            mods.Add(new ModInfo()
            {
                Name = "Recoil Overhaul",
                Author = "Fontaine",
                Description = "This is a substantial overhaul of how EFT's recoil system works. This recoil system works very similarly to Realism Mod's. If you already use that mod then you're not missing out on anything, don't install both.",
                ModUrl = "https://hub.sp-tarkov.com/files/file/953-fontaine-s-recoil-overhaul/",
                ModVersion = "2.2.0",
                SupportedVersion = "1.9.8713.31794",
                RequiresFiles = false
            });

            ModsList.ItemsSource = mods;
        }
    }
}
