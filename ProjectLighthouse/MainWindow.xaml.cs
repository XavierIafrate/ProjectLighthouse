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
            //DataContext = new MainViewModel();
            viewModel.window = this;
        }

        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton sender_button = sender as ToggleButton;
            // Uncheck toggle buttons
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
                button.IsChecked = button.Content == sender_button.Content;
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
            MessageBox.Show(string.Format("Debug Mode: {0}", System.Diagnostics.Debugger.IsAttached)); 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //debugButton.Visibility = Visibility.Collapsed; // App.currentUser.UserRole != "admin" ? Visibility.Collapsed : Visibility.Visible;
            assemblyOrders_button.IsEnabled = App.currentUser.UserRole == "admin";
            BOM_button.IsEnabled = App.currentUser.UserRole == "admin";
            assembly_button.IsEnabled = App.currentUser.UserRole == "admin";
        }
    }
}
