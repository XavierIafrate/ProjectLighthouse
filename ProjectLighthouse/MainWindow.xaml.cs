using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
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

            DebugTile.Visibility = App.DevMode ? Visibility.Collapsed : Visibility.Collapsed;
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
            manage_users_button.Visibility = App.CurrentUser.Role == Model.UserRole.Administrator
                ? Visibility.Visible
                : Visibility.Collapsed;

            manage_lathes_button.Visibility = App.CurrentUser.UserName == "xav"
                ? Visibility.Visible
                : Visibility.Collapsed;

            qc_button.Visibility = manage_lathes_button.Visibility;

            LoggedInUserName.Text = App.CurrentUser.GetFullName();
            LoggedInUserRole.Text = App.CurrentUser.UserRole;
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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
    }
}
