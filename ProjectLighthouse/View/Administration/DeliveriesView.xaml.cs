using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class DeliveriesView : UserControl
    {
        public DeliveriesView()
        {
            InitializeComponent();
            EditItemButton.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditDelivery) 
                ? System.Windows.Visibility.Visible 
                : System.Windows.Visibility.Collapsed;
        }

        private void clearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Text = "";
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.scroller.ScrollToTop();
        }
    }
}
