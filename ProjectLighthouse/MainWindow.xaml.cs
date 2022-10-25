using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using View.HelperWindows;

namespace ProjectLighthouse
{
    public partial class MainWindow : Window
    {
        public MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddVersionNumber()
        {
            if (App.CurrentUser == null)
            {
                return;
            }

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title = App.CurrentUser.Locale switch
            {
                "" => "Lighthouse",
                "Polish" => "Latarnia Morska",
                "Persian" => "فانوس دریایی",
                "Welsh" => "Goleudy",
                _ => "Lighthouse"
            };

            Title += $" v.{versionInfo.FileVersion}";

#if DEBUG
            Title += $" - {DatabaseHelper.DatabasePath}";
            userArea.Fill = (Brush)App.Current.Resources["Red"];
#endif

            DebugTile.Visibility = App.DevMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }
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

        public void SelectButton(string buttonName)
        {
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
            {
                button.IsChecked = (string)button.CommandParameter == buttonName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdminModule.Visibility = App.CurrentUser.Role == UserRole.Administrator
                ? Visibility.Visible
                : Visibility.Collapsed;

            LoggedInUserName.Text = App.CurrentUser.GetFullName();
            LoggedInUserRole.Text = App.CurrentUser.Role.ToString();
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //CalculationsHelperWindow test = new();
            //test.Show();

            AboutWindow window = new();
            window.ShowDialog();
        }

        private void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogoColour.Visibility = Visibility.Visible;
            LogoMono.Visibility = Visibility.Hidden;
        }

        private void Rectangle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogoColour.Visibility = Visibility.Hidden;
            LogoMono.Visibility = Visibility.Visible;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void viewPort_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            App.MainViewModel.NotificationsBarVis = Visibility.Collapsed;
        }

        private void OpenGithubButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName= "https://github.com/XavierIafrate/ProjectLighthouse",  UseShellExecute = true });
        }
    }
}
