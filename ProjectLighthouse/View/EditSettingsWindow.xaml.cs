using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for EditSettingsWindow.xaml
    /// </summary>
    public partial class EditSettingsWindow : Window
    {
        public User user { get; set; }
        public EditSettingsWindow()
        {
            InitializeComponent();
            user = App.currentUser;
            helperText.Visibility = Visibility.Collapsed;
            LoadText();
        }

        public void LoadText()
        {
            nameText.Text = user.GetFullName();
            userRoleText.Text = user.UserRole;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            helperText.Visibility = Visibility.Collapsed;

            if (current.Password != App.currentUser.Password)
            {
                helperText.Visibility = Visibility.Visible;
                helperText.Text = "Invalid Password";
                return;
            }

            if (newPwd.Password != confirmPwd.Password)
            {
                helperText.Visibility = Visibility.Visible;
                helperText.Text = "Passwords do not match";
                return;
            }

            user.Password = newPwd.Password;
            if (DatabaseHelper.Update<User>(user))
            {
                MessageBox.Show("Password successfully updated", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                newPwd.Password = "";
                confirmPwd.Password = "";
                current.Password = "";
            }
            else
            {
                MessageBox.Show("Failed to update password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
