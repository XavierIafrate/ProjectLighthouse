using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class BarStockView : UserControl
    {
        public BarStockView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Clear();
        }

        private void OrderListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewIssueButton.IsEnabled = OrderListView.SelectedValue != null;
        }
    }
}
