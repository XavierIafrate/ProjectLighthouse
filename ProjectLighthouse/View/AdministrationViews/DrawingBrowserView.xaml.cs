using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for DrawingBrowserView.xaml
    /// </summary>
    public partial class DrawingBrowserView : UserControl
    {
        public DrawingBrowserView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.IsEnabled = searchBox.Text.Length > 0;
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Text = "";
        }
    }
}
