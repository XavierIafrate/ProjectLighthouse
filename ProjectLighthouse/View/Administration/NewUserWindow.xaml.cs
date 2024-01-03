using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Administration
{
    public partial class NewUserWindow : Window
    {
        private List<User> currentUsers;
        private bool usernameValid;
        private bool firstNameValid;
        private bool lastNameValid;
        private bool passwordValid;
        public bool SaveExit;


        public NewUserWindow()
        {
            InitializeComponent();
            currentUsers = DatabaseHelper.Read<User>();
        }

        private void username_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(username.Text))
            {
                usernameValid = false;
                MarkValid(textbox, false);
                return;
            }

            if (currentUsers.Any(x => x.UserName == username.Text.Trim().ToLowerInvariant()))
            {
                usernameValid = false;
                MarkValid(textbox, false);
                return;
            }
            usernameValid = true;
            MarkValid(textbox, true);
        }

        private void lastName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(lastName.Text))
            {
                lastNameValid = false;
                MarkValid(textbox, false);
                return;
            }

            lastNameValid = true;
            MarkValid(textbox, true);
        }

        private void firstName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(firstName.Text))
            {
                firstNameValid = false;
                MarkValid(textbox, false);
                return;
            }

            firstNameValid = true;
            MarkValid(textbox, true);
        }


        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            passwordValid = password.Password.Length > 4;
            password.BorderBrush = passwordValid
                ? Brushes.Transparent
                : (Brush)Application.Current.Resources["Red"];
        }

        void MarkValid(TextBox textBox, bool valid)
        {
            textBox.BorderBrush = valid
                ? Brushes.Transparent
                : (Brush)Application.Current.Resources["Red"];
        }


        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            if (!usernameValid || !firstNameValid || !lastNameValid || !passwordValid)
            {
                return;
            }

            User user = new()
            {
                UserName = username.Text,
                FirstName = firstName.Text,
                LastName = lastName.Text,
                Password = password.Password,
                Role = UserRole.Viewer
            };

            DatabaseHelper.Insert(user);
            SaveExit = true;
            Close();
        }

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            username.Text = username.Text.Trim().ToLowerInvariant();
        }

        private void firstName_LostFocus(object sender, RoutedEventArgs e)
        {
            firstName.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName.Text.Trim());
        }

        private void lastName_LostFocus(object sender, RoutedEventArgs e)
        {
            lastName.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName.Text.Trim());
        }
    }
}
