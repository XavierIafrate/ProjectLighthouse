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


        public LoginWindow(bool logistics = false)
        {
            InitializeComponent();
            AddVersionNumber();

            Users = DatabaseHelper.Read<User>().ToList();

            LogisticsWarning.Visibility = logistics ? Visibility.Visible : Visibility.Hidden;

            if (Environment.UserName == "xavier" || Debugger.IsAttached)
            {
                User user = Users.Find(u => u.UserName == "xav");
                if (user == null)
                {
                    return;
                }
                UsernameTextBox.Text = user.UserName;
                PasswordBox.Password = user.Password;
                _ = PasswordBox.Focus();
                _ = PasswordBox.GetType()
                    .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(PasswordBox, new object[] { user.Password.Length, 0 });
                return;
            }

            string[] UsersOfThisComputer = Users.Where(u => u.computerUsername == Environment.UserName).Select(x => x.UserName).ToArray();

            if (UsersOfThisComputer.Length == 1)
            {
                UsernameTextBox.Text = UsersOfThisComputer[0];
                _ = PasswordBox.Focus();
                return;
            }
            else
            {
                UsernameTextBox.Focus();
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
            Users = DatabaseHelper.Read<User>().ToList();
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

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Login login = new()
            {
                User = User.UserName,
                Host = Environment.MachineName,
                ADUser = User.computerUsername,
                Role = User.UserRole,
                Time = User.LastLogin,
                Version = versionInfo.FileVersion
            };

            App.Login = login;

            if (!Debugger.IsAttached)
            {
                _ = DatabaseHelper.Insert(login);
            }
            auth_user = User;

            Close();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordGhost.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
               ? Visibility.Visible
               : Visibility.Hidden;
        }
    }
}
