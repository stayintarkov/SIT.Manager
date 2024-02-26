using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Controls
{
    public sealed partial class SelectDowngradePatcherMirrorDialog : ContentDialog
    {
        Dictionary<string, string> providerLinks = new Dictionary<string, string>();
        public string SelectedMirrorUrl = null;
        string releasesString;

        public SelectDowngradePatcherMirrorDialog(string sitVersionTarget)
        {
            this.InitializeComponent();

            var downloadingInfo = new Dictionary<string, string>
            {
                { "Downloading info...", "" }
            };

            MirrorBox.DataContext = downloadingInfo;
            MirrorBox.ItemsSource = downloadingInfo;
            
            MirrorBox.SelectedIndex = 0;
            
            FetchMirrors(sitVersionTarget);
        }

        public async void FetchMirrors(string sitVersionTarget)
        {
            if (App.ManagerConfig.TarkovVersion == null)
            {
                Loggy.LogToFile("DownloadPatcher: TarkovVersion is 'null'");
                return;
            }

            releasesString = await Utils.utilsHttpClient.GetStringAsync(@"https://sitcoop.publicvm.com/api/v1/repos/SIT/Downgrade-Patches/releases");
            List<GiteaRelease> giteaReleases = JsonSerializer.Deserialize<List<GiteaRelease>>(releasesString);
            if (giteaReleases == null)
            {
                Loggy.LogToFile("DownloadPatcher: giteaReleases is 'null'");
                return;
            }

            string tarkovBuild = App.ManagerConfig.TarkovVersion.Split(".").Last();
            string sitVersionTargetBuild = sitVersionTarget.Split(".").Last();

            GiteaRelease compatibleDowngradePatcher = null;

            foreach (var release in giteaReleases)
            {
                var releaseName = release.name;
                var patcherFrom = releaseName.Split(" to ")[0];
                var patcherTo = releaseName.Split(" to ")[1];

                if (patcherFrom == tarkovBuild && patcherTo == sitVersionTargetBuild)
                {
                    compatibleDowngradePatcher = release;
                    break;
                }
            }

            if(compatibleDowngradePatcher == null)
            {
                Loggy.LogToFile("No applicable patcher found for the specified SIT version.");
                return;
            }

            string mirrorsUrl = compatibleDowngradePatcher.assets.Find(q => q.name == "mirrors.json").browser_download_url;
            string mirrorsString = await Utils.utilsHttpClient.GetStringAsync(mirrorsUrl);
            List<Mirrors> mirrors = JsonSerializer.Deserialize<List<Mirrors>>(mirrorsString);
            
            if (mirrors == null || mirrors.Count == 0)
            {
                Loggy.LogToFile("No download mirrors found for patcher.");
                return;
            }

            foreach (var mirror in mirrors)
            {
                Uri uri = new Uri(mirror.Link);
                string host = uri.Host.Replace("www.", "").Split('.')[0];
                if (!providerLinks.ContainsKey(host))
                {
                    providerLinks.Add(host, mirror.Link);
                }
            }

            if(providerLinks.Keys.Count > 0)
            {
                MirrorBox.DataContext = providerLinks;
                MirrorBox.ItemsSource = providerLinks;

                MirrorBox.SelectedIndex = 0;
            }
        }

        private void MirrorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MirrorBox.SelectedIndex != -1)
            {
                string providerLink = MirrorBox.SelectedValue as string;

                if (!string.IsNullOrEmpty(providerLink)) 
                {
                    SelectedMirrorUrl = providerLink;
                }
            }
        }
    }
}
