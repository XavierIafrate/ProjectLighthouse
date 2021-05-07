using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
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
                    
                    if (Environment.UserName == "xavier")
                    {
                        passwordText.Password = user.Password;
                    }
                    if (String.IsNullOrEmpty(usernameText.Text))
                    {
                        usernameText.Text = user.UserName;
                        //passwordText.Focus();
                    }
                    else // multiple people using the computer
                    {
                        usernameText.Text = "";
                        passwordText.Password = "";
                    }
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DeliveryNote deliveryNote = new DeliveryNote()
            {
                Id = 1,
                Name = "D00359",
                DeliveryDate = DateTime.Now,
                DeliveredBy = "Xavier Iafrate"
            };

            List<DeliveryItem> deliveryItems = new List<DeliveryItem>()
            {
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                }, 
                new DeliveryItem()
                {
                    Id = 2,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-080-A2",
                    QuantityThisDelivery=500,
                    QuantityToFollow=500
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                },
                new DeliveryItem()
                {
                    Id = 1,
                    AllocatedDeliveryNote = deliveryNote.Name,
                    PurchaseOrderReference="P0123456",
                    Product="P0154.050-050-A2",
                    QuantityThisDelivery=1000,
                    QuantityToFollow=0
                }
            };

            PDFHelper.PrintDeliveryNote(deliveryNote, deliveryItems);
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
            message.Visibility = Visibility.Collapsed;
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
