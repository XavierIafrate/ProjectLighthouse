using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class LatheView : UserControl
    {
        public LatheView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void MaintenanceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView lst) return;
            EditMaintenanceButton.IsEnabled = lst.SelectedValue is MaintenanceEvent;
        }

        private void AttachmentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView lst) return;
            RemoveAttachmentButton.IsEnabled = lst.SelectedValue is Attachment;
        }
    }
}
