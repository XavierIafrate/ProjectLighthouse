using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System;
using System.Threading;
using System.Windows;
using System.Timers;
using ProjectLighthouse.ViewModel.Helpers;

namespace ProjectLighthouse
{
    public enum Skin { Classic, Dark }
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static Login Login { get; set; }
        public static string ROOT_PATH { get; set; }
        public static string ActiveViewModel { get; set; }
        public static DateTime LastDataRefresh { get; set; } = DateTime.MinValue;
        public static Skin Skin { get; set; } = Skin.Dark;
        public static System.Timers.Timer DataRefreshTimer { get; set; }
        public static bool DevMode { get; set; }

        public static MainViewModel MainViewModel { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseHelper.DatabasePath = Environment.UserName == "xavier"
                               ? @"C:\Users\xavie\Documents\lighthouse_test\manufactureDB.db3"
                               : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\manufactureDB.db3";

            DevMode = Environment.UserName == "xavier" || DatabaseHelper.DatabasePath.Contains("debug");

            MainWindow Window = new();
            MainViewModel VM = new()
            {
                MainWindow = Window
            };

            MainViewModel = VM;

            Window.DataContext = VM;
            Window.viewModel = VM;

            VM.LoginRoutine();
            Window.Show();

            Window.AddVersionNumber();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (Login != null)
                {
                    Login.Logout = DateTime.Now;
                    DatabaseHelper.Update(Login);
                }
            }
            finally
            {
                base.OnExit(e);
            }
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
