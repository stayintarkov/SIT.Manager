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
    public sealed partial class SelectSitVersionDialog : ContentDialog
    {
        public GithubRelease? version = null;
        string releasesString;
        List<GithubRelease>? githubReleases;
        List<GithubRelease> sitReleases = new();

        public SelectSitVersionDialog()
        {
            this.InitializeComponent();

            VersionBox.Items.Add(new GithubRelease()
            {
                tag_name = "下载信息中..."
            });
            VersionBox.SelectedIndex = 0;

            FetchReleases();
        }

        private async void FetchReleases()
        {
            try
            {
                releasesString = await Utils.utilsHttpClient.GetStringAsync(@"https://api.github.com/repos/stayintarkov/StayInTarkov.Client/releases");
                githubReleases = JsonSerializer.Deserialize<List<GithubRelease>>(releasesString);

                if (githubReleases.Count > 0)
                {
                    foreach (GithubRelease release in githubReleases)
                    {
                        Match match = Regex.Match(release.body, @"This version works with version [0]{1,}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,5}");
                        if (match.Success)
                        {
                            string releasePatch = match.Value.Replace("This version works with version ", "");
                            release.tag_name = release.name + " - Tarkov Version: " + releasePatch;
                            release.body = releasePatch;
                            release.assets.Find(q => q.name == "StayInTarkov-Release.zip").browser_download_url.Replace("github.com", "github.tarkov.free.hr");
                            sitReleases.Add(release);
                        }
                        else
                        {
                            Loggy.LogToFile("FetchReleases: There was a release without a version defined: " + release.html_url);
                        }
                    }

                    if (sitReleases.Count > 0)
                    {
                        VersionBox.Items.RemoveAt(0);

                        VersionBox.DataContext = sitReleases;
                        VersionBox.ItemsSource = sitReleases;
                        VersionBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    Loggy.LogToFile("InstallSIT: githubReleases was 0 for official branch");
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                Loggy.LogToFile("InstallSIT: " + ex.Message);
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
