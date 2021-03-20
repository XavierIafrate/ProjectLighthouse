using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            }
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
            message.Visibility = Visibility.Collapsed;
            foreach (User user in users)
            {
                if(user.UserName == usernameText.Text)
                {
                    if(user.Password == passwordText.Password)
                    {
                        if (user.IsBlocked)
                        {
                            message.Text = "User blocked";
                            message.Visibility = Visibility.Visible;
                            return;
                        }
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
    }
}
