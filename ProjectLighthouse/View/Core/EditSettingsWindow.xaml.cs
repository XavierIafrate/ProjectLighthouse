using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public partial class EditSettingsWindow : Window
    {
        public User user { get; set; }
        public List<PermissionOverview> permissionsYesNo;

        public EditSettingsWindow()
        {
            InitializeComponent();
            helperText.Visibility = Visibility.Collapsed;
            user = App.CurrentUser;
            DataContext = user;

            foreach (ComboBoxItem item in defaultViewComboBox.Items)
            {
                if ((string)item.Content == user.DefaultView)
                {
                    defaultViewComboBox.SelectedItem = item;
                    break;
                }
            }

            GetUserPermissions();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            helperText.Visibility = Visibility.Collapsed;

            if (current.Password != App.CurrentUser.Password)
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

            if (newPwd.Password.Length < 5)
            {
                helperText.Visibility = Visibility.Visible;
                helperText.Text = "Password is not long enough";
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

        private void defaultViewComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            ComboBoxItem item = (ComboBoxItem)comboBox.SelectedValue;
            user.DefaultView = item.Content.ToString();
            DatabaseHelper.Update<User>(user);
        }

        private void confirmPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            confirm_password_ghost.Visibility = confirmPwd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void current_PasswordChanged(object sender, RoutedEventArgs e)
        {
            current_password_ghost.Visibility = current.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void newPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            new_password_ghost.Visibility = newPwd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        void GetUserPermissions()
        {
            permissionsYesNo = new();

            foreach (PermissionType i in Enum.GetValues(typeof(PermissionType)))
            {
                permissionsYesNo.Add(item: new()
                {
                    Action = i,
                    DisplayText = Permission.PermissionNames[i],
                    UserHasPermission = App.CurrentUser.HasPermission(i)
                });
            }
            PermissionsList.ItemsSource = permissionsYesNo;
        }

        public class PermissionOverview
        {
            public PermissionType Action { get; set; }
            public string DisplayText { get; set; }
            public bool UserHasPermission { get; set; }
        }
    }
}
