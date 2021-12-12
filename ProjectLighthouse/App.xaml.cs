using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Threading;
using System.Windows;

namespace ProjectLighthouse
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static string ROOT_PATH { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CheckForExistingInstances();

            MainWindow Window = new();
            MainViewModel VM = new()
            {
                MainWindow = Window
            };

            Window.DataContext = VM;
            Window.viewModel = VM;

            VM.LoginRoutine();
            Window.Show();

            Window.AddVersionNumber();
        }

        private static void CheckForExistingInstances()
        {
            const string appName = "ProjectLighthouse";
            _ = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show("An existing instance of ProjectLighthouse is running.",
                    "Process Terminated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);

                Application.Current.Shutdown();
            }
        }
    }
}
