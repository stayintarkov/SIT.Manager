using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SIT.Manager.Controls
{
    public sealed partial class FontFamilyPickerDialog : ContentDialog
    {
        public string selectedFontFamily;

        private string currentFontStyle = App.ManagerConfig.ConsoleFontFamily;

        private string[] fontFamilyNames = {
            "Arial", "Calibri", "Cambria", "Cambria Math", "Comic Sans MS", "Consolas",
            "Courier New", "Ebrima", "Gadugi", "Georgia",
            "Javanese Text Regular Fallback font for Javanese script", "Leelawadee UI",
            "Lucida Console", "Malgun Gothic", "Microsoft Himalaya", "Microsoft JhengHei",
            "Microsoft JhengHei UI", "Microsoft New Tai Lue", "Microsoft PhagsPa",
            "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei UI",
            "Microsoft Yi Baiti", "Mongolian Baiti", "MV Boli", "Myanmar Text",
            "Nirmala UI", "Segoe MDL2 Assets", "Segoe Print", "Segoe UI", "Segoe UI Emoji",
            "Segoe UI Historic", "Segoe UI Symbol", "SimSun", "Times New Roman",
            "Trebuchet MS", "Verdana", "Webdings", "Wingdings", "Yu Gothic",
            "Yu Gothic UI"
        };

        public FontFamilyPickerDialog()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void FontFamilyOKButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            selectedFontFamily = FontFamilyComboBox.SelectedItem.ToString();
            this.Hide();
        }

        private void FontFamilyCancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
