using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Controls
{
    public sealed partial class SelectServerVersionDialog : ContentDialog
    {
        public GithubRelease? version = null;
        string releasesString;
        List<GithubRelease>? githubReleases;
        List<GithubRelease> serverReleases = new();

        public SelectServerVersionDialog()
        {
            this.InitializeComponent();

            VersionBox.Items.Add(new GithubRelease()
            {
                tag_name = "Downloading info..."
            });
            VersionBox.SelectedIndex = 0;

            FetchReleases();
        }

        private async void FetchReleases()
        {
            try
            {
                releasesString = await Utils.utilsHttpClient.GetStringAsync(@"https://api.github.com/repos/stayintarkov/SIT.Aki-Server-Mod/releases");
                githubReleases = JsonSerializer.Deserialize<List<GithubRelease>>(releasesString);

                if (githubReleases.Count > 0)
                {
                    foreach (GithubRelease release in githubReleases)
                    {
                        var zipAsset = release.assets.Find(asset => asset.name.EndsWith(".zip"));
                        if (zipAsset != null) // There is a .zip asset in this release
                        {
                            Match match = Regex.Match(release.body, @"This server version works with EFT version ([0]{1,}\.[0-9]{1,2}\.[0-9]{1,2})\.[0-9]{1,2}\.[0-9]{1,5}");
                            if (match.Success)
                            {
                                string releasePatch = match.Groups[1].Value;
                                release.tag_name = release.name + " - Tarkov Version: " + releasePatch;
                                release.body = releasePatch;
                                serverReleases.Add(release);
                            }
                            else
                            {
                                Loggy.LogToFile("FetchReleases: There was a release without a version defined: " + release.html_url);
                            }
                        }
                    }

                    if (serverReleases.Count > 0)
                    {
                        VersionBox.Items.RemoveAt(0);

                        VersionBox.DataContext = serverReleases;
                        VersionBox.ItemsSource = serverReleases;
                        VersionBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    Loggy.LogToFile("Install Server: githubReleases was 0 for official branch");
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                Loggy.LogToFile("Install Server: " + ex.Message);
            }
        }

        private void VersionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VersionBox.SelectedIndex != -1)
            {
                version = VersionBox.SelectedItem as GithubRelease;
            }
        }
    }
}
