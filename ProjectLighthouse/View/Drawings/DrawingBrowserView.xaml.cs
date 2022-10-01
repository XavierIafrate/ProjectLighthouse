using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Drawings
{
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

        private void Message_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton.Command.Execute(null);
            }
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            archtypeAnnotations.Visibility = this.ActualWidth > 1380 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
    }
}
