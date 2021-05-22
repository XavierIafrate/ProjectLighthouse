using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ProjectLighthouse
{
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as MainViewModel;
            DataContext = new MainViewModel();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton sender_button = sender as ToggleButton;
            // Uncheck toggle buttons
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
                button.IsChecked = button == sender_button;
            
            // just to be sure
            sender_button.IsChecked = true;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;
                    
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //return;
            //EmailHelper.SendEmail("x.iafrate@wixroydgroup.com", "Request Approved", "This is a test of SMTP integration");
            //Request tmpRequst = new Request()
            //{
            //    QuantityRequired = 100,
            //    Product = "TEST",
            //    RaisedBy = "Xavier Iafrate",
            //    DeclinedReason="Uneconomical"
            //};

            //EmailHelper.NotifyRequestDeclined(tmpRequst);

            List<LatheManufactureOrderItem> items = new List<LatheManufactureOrderItem>()
            {
                new LatheManufactureOrderItem()
                {
                    ProductName="Test1",
                    TargetQuantity = 1000,
                    RequiredQuantity = 100,
                    DateRequired = DateTime.Now
                },
                new LatheManufactureOrderItem()
                {
                    ProductName="Test2",
                    TargetQuantity = 1000,
                    RequiredQuantity = 0,
                    DateRequired = DateTime.Now
                }
            };

            LatheManufactureOrder order = new LatheManufactureOrder()
            {
                Name = "TESTORDER"
            };

            EmailHelper.NotifyNewOrder(order, items);
            return;


            //List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            //List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>();
            //List<Lathe> lathes = DatabaseHelper.Read<Lathe>();

            //List<LatheManufactureOrder> activeOrders = new List<LatheManufactureOrder>();
            //List<LatheManufactureOrderItem> activeItems = new List<LatheManufactureOrderItem>();

            //List<string> active_order_names = new List<string>();

            //foreach(LatheManufactureOrder order in orders)
            //{
            //    if (order.Status != "Complete")
            //    {
            //        activeOrders.Add(order);
            //        active_order_names.Add(order.Name);
            //    }
            //}

            //foreach(LatheManufactureOrderItem item in items)
            //{
            //    if (active_order_names.Contains(item.AssignedMO))
            //        activeItems.Add(item);
            //}
            //PDFHelper.PrintSchedule(activeOrders, activeItems, lathes);

            //ReportingHelper.GetReport();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugButton.Visibility = App.currentUser.UserRole != "admin" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
