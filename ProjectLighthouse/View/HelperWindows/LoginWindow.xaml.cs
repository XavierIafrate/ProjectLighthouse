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
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public User auth_user;

        public List<User> users;
        public LoginWindow()
        {
            InitializeComponent();
            AddVersionNumber();

            users = DatabaseHelper.Read<User>().ToList();
            foreach (User user in users)
            {
                if (user.computerUsername == Environment.UserName)
                {
                    if (string.IsNullOrEmpty(usernameText.Text))
                    {
                        usernameText.Text = user.UserName;
                        PasswordBox.Focus();
                    }
                    else // multiple people using the computer
                    {
                        usernameText.Text = "";
                        PasswordBox.Password = "";
                        usernameText.Focus();
                    }

                    if (Environment.UserName == "xavier")
                    {
                        PasswordBox.Password = user.Password;
                        PasswordBox.GetType().GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(PasswordBox, new object[] { user.Password.Length, 0 });
                    }

                }
            }

            if (string.IsNullOrEmpty(usernameText.Text))
                usernameText.Focus();
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
                Login();
        }

        private void Login()
        {
            List<User> matches = users.Where(x => x.UserName == usernameText.Text).ToList();
            User user;

            if (matches.Count == 0)
            {
                message.Text = "Username not found";
                MessageBadge.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                user = matches.First();
            }

            if (user.IsBlocked)
            {
                message.Text = "User blocked";
                MessageBadge.Visibility = Visibility.Visible;
                return;
            }
            else if (user.Password == PasswordBox.Password)
            {
                user.LastLogin = DateTime.Now;
                user.computerUsername = Environment.UserName;
                DatabaseHelper.Update<User>(user);

                auth_user = user;
                Close();
            }
            else
            {
                message.Text = "Invalid password";
                MessageBadge.Visibility = Visibility.Visible;
            }
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordGhost.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
               ? Visibility.Visible
               : Visibility.Hidden;
        }

        private void usernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernameGhost.Visibility = string.IsNullOrEmpty(usernameText.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
