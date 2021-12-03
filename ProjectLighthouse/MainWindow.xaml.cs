using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
            App.ROOT_PATH = Environment.UserName == "xavier" ? @"C:\Users\xavie\Desktop\" : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\";

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
            Title += $" - {DatabaseHelper.GetDatabaseFile()}";
            DebugTile.Visibility = Visibility.Visible;
#endif
        }

        public async Task CheckForUpdates()
        {
            using (UpdateManager manager = new(@"\\groupfile01\Production\Administration\Manufacture Records\Lighthouse\Release"))
            {
                _ = await manager.UpdateApp();
            };
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
            manage_users_button.Visibility = App.CurrentUser.UserRole == "admin" ? Visibility.Visible : Visibility.Collapsed;
            debug_button.Visibility = App.CurrentUser.UserName == "xav" ? Visibility.Visible : Visibility.Collapsed;

            LoggedInUserName.Text = App.CurrentUser.GetFullName();
            LoggedInUserRole.Text = App.CurrentUser.UserRole;
        }

        private async void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.ToString() == "F8")
            {
                if (MessageBox.Show("Are you sure you want to update stock levels?", "Lighthouse Opera Sync", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    string logPath = @"C:\Users\x.iafrate\Documents\sync_log.txt";

                    if (File.Exists(OperaHelper.dbFile))
                    {
                        Console.WriteLine("Proceeding");

                        List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();
                        List<BarStock> bar = DatabaseHelper.Read<BarStock>();
                        string[] lookup = new string[products.Count];

                        for (int i = 0; i < products.Count; i++)
                            lookup[i] = products[i].ProductName;

                        OperaHelper.UpdateRecords(products, bar, lookup);

                        File.AppendAllText(logPath, $"{DateTime.Now:s} - pass\n");
                    }
                    else
                    {
                        Console.WriteLine("Failed to locate db");
                        File.AppendAllText(logPath, $"{DateTime.Now:s} - fail\n");
                        Console.ReadLine();
                    }
                    MessageBox.Show("Complete");
                }
            }
        }
    }
}
