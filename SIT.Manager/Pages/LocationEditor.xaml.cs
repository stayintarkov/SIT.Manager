using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;
using Windows.Storage.Pickers;
using System;
using Windows.Storage;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocationEditor : Page
    {
        public LocationEditor()
        {
            this.InitializeComponent();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new() 
            {
                FileTypeFilter = { ".json" }
            };

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file == null)
                return;

            if (File.Exists(file.Path))
            {
                BaseLocation location = JsonSerializer.Deserialize<BaseLocation>(File.ReadAllText(file.Path));

                if (location == null)
                {
                    Utils.ShowInfoBar("加载错误", "加载文件时出错。", InfoBarSeverity.Error);
                    return;
                }

                for (int i = 0; i < location.waves.Count; i++)
                {
                    location.waves[i].Name = i + 1;
                }

                for (int i = 0; i < location.BossLocationSpawn.Count; i++)
                {
                    location.BossLocationSpawn[i].Name = i + 1;
                }

                switch (location.Scene.path)
                {
                    case "maps/factory_day_preset.bundle":
                        LocationTextBox.Text = "工厂 (白图)";
                        break;
                    case "maps/factory_night_preset.bundle":
                        LocationTextBox.Text = "工厂 (夜图)";
                        break;
                    case "maps/woods_preset.bundle":
                        LocationTextBox.Text = "森林";
                        break;
                    case "maps/customs_preset.bundle":
                        LocationTextBox.Text = "海关";
                        break;
                    case "maps/shopping_mall.bundle":
                        LocationTextBox.Text = "立交桥";
                        break;
                    case "maps/rezerv_base_preset.bundle":
                        LocationTextBox.Text = "储备站";
                        break;
                    case "maps/shoreline_preset.bundle":
                        LocationTextBox.Text = "海岸线";
                        break;
                    case "maps/laboratory_preset.bundle":
                        LocationTextBox.Text = "实验室";
                        break;
                    case "maps/lighthouse_preset.bundle":
                        LocationTextBox.Text = "灯塔";
                        break;
                    case "maps/city_preset.bundle":
                        LocationTextBox.Text = "塔科夫街区";
                        break;
                    default:
                        break;
                }

                DataContext = location;

                if (location.waves.Count > 0)
                {
                    WaveList.SelectedIndex = 0;
                }

                if (location.BossLocationSpawn.Count > 0)
                {
                    BossList.SelectedIndex = 0;
                }

                Utils.ShowInfoBar("加载地图配置", $"地图配置 {LocationTextBox.Text} 已成功加载。", InfoBarSeverity.Success);
            }
                
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker filePicker = new()
            {
                DefaultFileExtension = ".json",
                SuggestedFileName = "base.json",
                FileTypeChoices = { { "JSON", new List<string>() { ".json" } } }
            };

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSaveFileAsync();

            if (file == null)
                return;

            if (File.Exists(file.Path))
            {
                File.Copy(file.Path, file.Path.Replace(".json", ".BAK"), true);
            }

            BaseLocation baseLocation = (BaseLocation)DataContext;
            if (baseLocation == null)
            {
                Utils.ShowInfoBar("保存错误", "保存文件时出错。", InfoBarSeverity.Error);
                return;
            }
            var json = JsonSerializer.Serialize(baseLocation, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(file.Path, json);
            Utils.ShowInfoBar("保存", $"已成功保存至: {file.Path}", InfoBarSeverity.Success);
        }

        private void AddWaveButton_Click(object sender, RoutedEventArgs e)
        {
            BaseLocation location = (BaseLocation)DataContext;

            if (location != null)
            {
                location.waves.Add(new Wave());

                for (int i = 0; i < location.waves.Count; i++)
                {
                    location.waves[i].Name = i + 1;
                }

                if (location.waves.Count > 0)
                {
                    WaveList.SelectedIndex = 0;
                }
            }
        }

        private void RemoveWaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (WaveList.SelectedIndex == -1)
                return;

            BaseLocation location = (BaseLocation)DataContext;

            if (location != null)
            {
                location.waves.RemoveAt(WaveList.SelectedIndex);

                for (int i = 0; i < location.waves.Count; i++)
                {
                    location.waves[i].Name = i + 1;
                }

                if (location.waves.Count > 0)
                {
                    WaveList.SelectedIndex = 0;
                }
            }
        }

        private void AddBossButton_Click(object sender, RoutedEventArgs e)
        {
            BaseLocation location = (BaseLocation)DataContext;

            if (location != null)
            {
                location.BossLocationSpawn.Add(new BossLocationSpawn());

                for (int i = 0; i < location.BossLocationSpawn.Count; i++)
                {
                    location.BossLocationSpawn[i].Name = i + 1;
                }

                if (location.BossLocationSpawn.Count > 0)
                {
                    BossList.SelectedIndex = 0;
                }
            }
        }

        private void RemoveBossButton_Click(object sender, RoutedEventArgs e)
        {
            if (BossList.SelectedIndex == -1)
                return;

            BaseLocation location = (BaseLocation)DataContext;

            if (location != null)
            {
                location.BossLocationSpawn.RemoveAt(BossList.SelectedIndex);

                for (int i = 0; i < location.BossLocationSpawn.Count; i++)
                {
                    location.BossLocationSpawn[i].Name = i + 1;
                }

                if (location.BossLocationSpawn.Count > 0)
                {
                    BossList.SelectedIndex = 0;
                }
            }
        }
    }
}
