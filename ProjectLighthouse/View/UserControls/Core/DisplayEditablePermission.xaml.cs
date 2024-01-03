using ProjectLighthouse.Model.Core;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayEditablePermission : UserControl
    {
        public EditablePermission Permission
        {
            get { return (EditablePermission)GetValue(PermissionProperty); }
            set { SetValue(PermissionProperty, value); }
        }

        public static readonly DependencyProperty PermissionProperty =
            DependencyProperty.Register("Permission", typeof(EditablePermission), typeof(DisplayEditablePermission), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayEditablePermission control) return;

            control.PermissionName.Text = control.Permission.DisplayText;
            control.HasPermissionBadge.Visibility = control.Permission.UserHasPermission ? Visibility.Visible : Visibility.Collapsed;
            control.NoPermissionBadge.Visibility = !control.Permission.UserHasPermission ? Visibility.Visible : Visibility.Collapsed;
            control.GrantedByRoleBadge.Visibility = control.Permission.Inherited ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayEditablePermission()
        {
            InitializeComponent();
        }
    }
}
