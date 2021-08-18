using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class LoginWindow : Window
    {
        public User auth_user;

        public List<User> Users;
        public LoginWindow()
        {
            InitializeComponent();
            AddVersionNumber();

            Users = DatabaseHelper.Read<User>().ToList();

            if (Environment.UserName == "xavier" || Debugger.IsAttached)
            {
                User user = Users.SingleOrDefault(u => u.UserName == "xav");
                if (user == null)
                {
                    return;
                }
                UsernameTextBox.Text = user.UserName;
                PasswordBox.Password = user.Password;
                _ = PasswordBox.Focus();
                _ = PasswordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(PasswordBox, new object[] { user.Password.Length, 0 });
                return;
            }

            List<User> UsersOfThisComputer = Users.Where(u => u.computerUsername == Environment.UserName).ToList();

            if (UsersOfThisComputer.Count == 1)
            {
                UsernameTextBox.Text = UsersOfThisComputer.Single().UserName;
                _ = PasswordBox.Focus();
                return;
            }
        }

        private void AddVersionNumber()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            VersionInfo.Text = $"v{FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion}";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void PasswordText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Login();
            }
        }

        private void Login()
        {
            User User = Users.FirstOrDefault(u => u.UserName == UsernameTextBox.Text);

            if (User == null)
            {
                message.Text = "Username not found";
                MessageBadge.Visibility = Visibility.Visible;
                return;
            }
            else if (User.IsBlocked)
            {
                message.Text = "User blocked";
                MessageBadge.Visibility = Visibility.Visible;
                return;
            }
            else if (User.Password != PasswordBox.Password)
            {
                message.Text = "Invalid password";
                MessageBadge.Visibility = Visibility.Visible;
                return;
            }

            User.LastLogin = DateTime.Now;
            User.computerUsername = Environment.UserName;
            _ = DatabaseHelper.Update(User);

            auth_user = User;

            Close();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordGhost.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
               ? Visibility.Visible
               : Visibility.Hidden;
        }

        private void UsernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernameGhost.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
