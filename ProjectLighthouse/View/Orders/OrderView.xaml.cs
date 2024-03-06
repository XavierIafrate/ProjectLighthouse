using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders
{
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.IsEnabled = !string.IsNullOrEmpty(SearchBox.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }
    }
}
