using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Controls
{
    public sealed partial class SelectEditionDialog : ContentDialog
    {
        public string edition = "";

        public SelectEditionDialog()
        {
            this.InitializeComponent();
            EditionBox.SelectedIndex = 0;
        }

        private void EditionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditionBox.SelectedIndex != -1)
            {
                edition = ((ComboBoxItem)EditionBox.SelectedItem).Content.ToString();
            }
        }
    }
}
