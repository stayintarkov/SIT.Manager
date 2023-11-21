using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Controls
{
    public sealed partial class ColorPickerDialog : ContentDialog
    {
        public Color SelectedColor;

        public ColorPickerDialog()
        {
            this.InitializeComponent();

            ColorPickerControl.Color = App.ManagerConfig.ConsoleFontColor;
        }

        private void ColorPickerControlSelectButton(object sender, RoutedEventArgs e)
        {
            SelectedColor = ColorPickerControl.Color;
            Hide();
        }
        private void ColorPickerControlCancelButton(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
