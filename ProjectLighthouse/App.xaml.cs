using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System;
using System.Threading;
using System.Windows;
using System.Timers;

namespace ProjectLighthouse
{
    public enum Skin { Classic, Dark }
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static string ROOT_PATH { get; set; }
        public static string ActiveViewModel { get; set; }
        public static DateTime LastDataRefresh { get; set; } = DateTime.MinValue;
        public static Skin Skin { get; set; } = Skin.Dark;
        public static System.Timers.Timer DataRefreshTimer { get; set; }
        private static MainViewModel mvm { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CheckForExistingInstances();

            MainWindow Window = new();
            MainViewModel VM = new()
            {
                MainWindow = Window
            };

            mvm = VM;

            Window.DataContext = VM;
            Window.viewModel = VM;

            VM.LoginRoutine();
            Window.Show();

            Window.AddVersionNumber();

            StartDataRefreshTimer();
        }

        private static void StartDataRefreshTimer()
        {
            DataRefreshTimer = new();
            DataRefreshTimer.Elapsed += DataRefreshTimer_Elapsed;
            DataRefreshTimer.Interval = 5000;
            DataRefreshTimer.Enabled = true;
            //DataRefreshTimer.Start();
        }

        private static void DataRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mvm.LastDataRefresh = $"{DateTime.Now:s}";
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
