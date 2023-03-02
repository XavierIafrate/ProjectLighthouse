using DocumentFormat.OpenXml.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class ProductManagerView : UserControl
    {
        public ProductManagerView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Clear();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.IsEnabled = !string.IsNullOrEmpty(searchBox.Text);
        }
    }
}
