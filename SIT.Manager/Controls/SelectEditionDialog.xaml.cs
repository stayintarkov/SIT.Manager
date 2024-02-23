using Microsoft.UI.Xaml.Controls;
using SIT.Manager.Classes;

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
            EditionBox.Items.Clear();
            for (int i = 0; i < AkiServer.Editions.Length; i++)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = AkiServer.Editions[i];
                EditionBox.Items.Add(comboBoxItem);
            }
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
