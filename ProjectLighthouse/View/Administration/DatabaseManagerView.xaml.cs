using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class DatabaseManagerView : UserControl
    {
        public DatabaseManagerView()
        {
            InitializeComponent();

            usersTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditUsers) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            lathesTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageLathes) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            materialsTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditMaterials) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            reportsTab.Visibility = App.CurrentUser.HasPermission(Model.Core.PermissionType.ViewReports) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        }
    }
}
