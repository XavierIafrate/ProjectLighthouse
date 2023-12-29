using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Requests
{

    public partial class RequestView : UserControl
    {
        public RequestView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;
            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.MessageTextBox.Text) && e.Key == Key.Enter)
            {
                SendButton.Command.Execute(null);
            }
        }

        private void requests_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            scroller?.ScrollToVerticalOffset(0);
        }
    }
}
