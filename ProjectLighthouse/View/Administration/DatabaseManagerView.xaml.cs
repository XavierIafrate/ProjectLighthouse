using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class DatabaseManagerView : UserControl
    {
        public DatabaseManagerView()
        {
            InitializeComponent();

        }

        private void TabControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            usersTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditUsers) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            lathesTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageLathes) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            materialsTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditMaterials) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            reportsTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.ViewReports) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            if (usersTab.Visibility == System.Windows.Visibility.Visible)
            {
                TabControl.SelectedIndex = 0;
            }
            else if (lathesTab.Visibility == System.Windows.Visibility.Visible)
            {
                TabControl.SelectedIndex = 1;
            }
            else if (materialsTab.Visibility == System.Windows.Visibility.Visible)
            {
                TabControl.SelectedIndex = 2;
            }
            else if (reportsTab.Visibility == System.Windows.Visibility.Visible)
            {
                TabControl.SelectedIndex = 3;
            }
        }
    }
}
