using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System;
using System.Threading;
using System.Windows;
using System.Timers;
using ProjectLighthouse.ViewModel.Helpers;
using System.Diagnostics;
using ProjectLighthouse.View;
using System.IO;
using System.Collections.Generic;

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
            
            //this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            DatabaseHelper.DatabasePath = Environment.UserName == "xavier"
                               ? @"C:\Users\xavie\Documents\lighthouse_test\manufactureDB.db3"
                               : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\manufactureDB.db3";

            DevMode = Debugger.IsAttached;

            ROOT_PATH = Environment.UserName == "xavier"
                ? @"C:\Users\xavie\Documents\lighthouse_test\"
                : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\";

            if (!Directory.Exists(ROOT_PATH))
            {
                MessageBox.Show("Could not locate root directory.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            string workstation = GetWorkstationLogistics();
            if (string.IsNullOrEmpty(workstation))
            {
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
            else
            {

                LogisticsKioskWindow window = new();
                window.WorkStation = workstation;
                window.LoginRoutine();
            }
        }

        private string? GetWorkstationLogistics()
        {
            string jsonLogisticsMachines = File.ReadAllText($"{ROOT_PATH}/PackingStations.json");
            Dictionary<string, string> machines = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonLogisticsMachines);
            foreach (string key in machines.Keys)
            {
                if (key.ToUpper() == Environment.MachineName.ToUpper())
                {
                    return machines[key];
                }
            }

            return null;
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

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            ShowError errorWindow = new() { Error = e };
            errorWindow.NotifyPropertyChanged();
            errorWindow.ShowDialog();
        }
    }
}
