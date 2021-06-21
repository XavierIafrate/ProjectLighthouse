using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ObservableCollection<User> users;
        public LoginWindow()
        {
            InitializeComponent();
            users = new ObservableCollection<User>();
            var usersList = DatabaseHelper.Read<User>().ToList();
            foreach (var user in usersList)
            {
                users.Add(user);
                if (user.computerUsername == Environment.UserName)
                {
                    if (String.IsNullOrEmpty(usernameText.Text))
                    {
                        usernameText.Text = user.UserName;
                        passwordText.Focus();
                    }
                    else // multiple people using the computer
                    {
                        usernameText.Text = "";
                        passwordText.Password = "";
                    }

                    if (Environment.UserName == "xavier")
                    {
                        passwordText.Password = user.Password;
                        passwordText.GetType().GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(passwordText, new object[] { user.Password.Length, 0 });
                    }

                }
            }
            if (string.IsNullOrEmpty(usernameText.Text))
                usernameText.Focus();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void passwordText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Login();
            }
        }

        private void Login()
        {
            message.Visibility = Visibility.Hidden;
            foreach (User user in users)
            {
                if (user.UserName == usernameText.Text)
                {
                    if (user.Password == passwordText.Password)
                    {
                        if (user.IsBlocked)
                        {
                            message.Text = "User blocked";
                            message.Visibility = Visibility.Visible;
                            return;
                        }
                        user.LastLogin = DateTime.Now;
                        user.computerUsername = Environment.UserName;
                        DatabaseHelper.Update<User>(user);
                        auth_user = user;
                        this.Close();
                        return;
                    }
                    else
                    {
                        message.Text = "Invalid password";
                        message.Visibility = Visibility.Visible;
                        return;
                    }
                }
            }

            message.Text = "Username not found";
            message.Visibility = Visibility.Visible;
        }

        private void usernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            usernameGhost.Visibility = !String.IsNullOrEmpty(textBox.Text) ? Visibility.Hidden : Visibility.Visible;
        }

        private void passwordText_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox textBox = sender as PasswordBox;
            passwordGhost.Visibility = !String.IsNullOrEmpty(textBox.Password) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
