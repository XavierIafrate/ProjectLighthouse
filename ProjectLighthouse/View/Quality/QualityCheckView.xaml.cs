using System.Windows.Controls;

namespace ProjectLighthouse.View.Quality
{
    public partial class QualityCheckView : UserControl
    {
        public QualityCheckView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }
            SendButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text);
        }

        private void clearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Clear();
        }
    }
}
