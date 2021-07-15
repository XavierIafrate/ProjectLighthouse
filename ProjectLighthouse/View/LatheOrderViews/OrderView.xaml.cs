using System.Drawing;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.Visibility = string.IsNullOrEmpty(SearchBox.Text) 
                ? System.Windows.Visibility.Hidden 
                : System.Windows.Visibility.Visible;

            SearchGhost.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Hidden;
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }
    }
}
