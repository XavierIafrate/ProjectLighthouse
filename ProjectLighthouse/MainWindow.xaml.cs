using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ProjectLighthouse
{
    public partial class MainWindow : Window
    {
        public MainViewModel viewModel;

        public MainWindow()
        {
            App.ROOT_PATH = Environment.UserName == "xavier"
                ? @"C:\Users\xavie\Documents\lighthouse_test\"
                : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\";

            if (!Directory.Exists(App.ROOT_PATH))
            {
                MessageBox.Show("Could not locate root directory.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();

            if (App.CurrentUser == null)
            {
                return;
            }
        }

        public void AddVersionNumber()
        {
            if (App.CurrentUser == null)
            {
                return;
            }

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title += $" v.{versionInfo.FileVersion}";

            #if DEBUG
            Title += $" - {DatabaseHelper.DatabasePath}";
            #endif

            DebugTile.Visibility = App.DevMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is not null and T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public void SelectButton(string buttonName)
        {
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
            {
                button.IsChecked = (string)button.CommandParameter == buttonName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            manage_users_button.Visibility = App.CurrentUser.UserRole == "admin"
                ? Visibility.Visible
                : Visibility.Collapsed;

            //debug_button.Visibility = App.CurrentUser.UserName == "xav"
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;

            LoggedInUserName.Text = App.CurrentUser.GetFullName();
            LoggedInUserRole.Text = App.CurrentUser.UserRole;
        }

        private void Image_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle rect)
            {
                LogoFill.Visibility = rect.IsMouseDirectlyOver ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AboutWindow window = new();
            window.ShowDialog();
        }
    }
}
