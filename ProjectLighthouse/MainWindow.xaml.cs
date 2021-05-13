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
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
            {
                button.IsChecked = button == sender_button;
            }
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
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();

            List<LatheManufactureOrder> activeOrders = new List<LatheManufactureOrder>();
            List<LatheManufactureOrderItem> activeItems = new List<LatheManufactureOrderItem>();

            List<string> active_order_names = new List<string>();

            foreach(LatheManufactureOrder order in orders)
            {
                if (order.Status != "Complete")
                {
                    activeOrders.Add(order);
                    active_order_names.Add(order.Name);
                }
            }

            foreach(LatheManufactureOrderItem item in items)
            {
                if (active_order_names.Contains(item.AssignedMO))
                    activeItems.Add(item);
            }
            PDFHelper.PrintSchedule(activeOrders, activeItems, lathes);

            //ReportingHelper.GetReport();
            //EmailHelper.SendEmail("xavieriafrate@gmail.com", "TEST", "This is a test of SMTP integration");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugButton.Visibility = App.currentUser.UserRole != "admin" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
