using ProjectLighthouse.ViewModel;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProjectLighthouse
{
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;

        public MainWindow()
        {
            App.ROOT_PATH = Environment.UserName == "xavier" ? @"C:\Users\xavie\Desktop\" : @"H:\Production\Administration\Manufacture Records\Lighthouse\";
            InitializeComponent();

            if (App.currentUser == null)
                return;
            
            viewModel = Resources["vm"] as MainViewModel;
            viewModel.window = this;
            viewModel.UpdateViewCommand.Execute(App.currentUser.DefaultView ?? "Orders");

            //Squirrel --releasify Lighthouse.1.0.0.nupkg
            AddVersionNumber();
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CheckForUpdates();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.Title += $" v.{versionInfo.FileVersion}";
            File.AppendAllText(Path.Join(App.ROOT_PATH, "log.txt"), $"{App.currentUser.UserName} login at {DateTime.Now:dd/MM/yy HH:mm:ss} with version {versionInfo.FileVersion}\n");
        }

        private async Task CheckForUpdates()
        {
            using (var manager = new UpdateManager(@"H:\Production\Administration\Manufacture Records\Lighthouse\Release"))
            {
                await manager.UpdateApp();
            };
        }

        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            
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

        public void SelectButton(string buttonName)
        {
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
                button.IsChecked = (string)button.CommandParameter == buttonName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format("Debug Mode: {0}", Debugger.IsAttached));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            debugButton.Visibility = Visibility.Collapsed; // App.currentUser.UserRole != "admin" ? Visibility.Collapsed : Visibility.Visible;
            assemblyOrders_button.IsEnabled = App.currentUser.UserRole == "admin";
            BOM_button.IsEnabled = App.currentUser.UserRole == "admin";
            assembly_button.IsEnabled = App.currentUser.UserRole == "admin";

        }
    }
}
