using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;

namespace ProjectLighthouse
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static Login Login { get; set; }
        public static string ROOT_PATH { get; set; }
        public static string ActiveViewModel { get; set; }
        public static DateTime LastDataRefresh { get; set; } = DateTime.MinValue;
        public static System.Timers.Timer DataRefreshTimer { get; set; }
        public static bool DevMode { get; set; }
        private static MainWindow Window;
        public static MainViewModel MainViewModel { get; set; }
        public static NotificationManager NotificationsManager { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            ROOT_PATH = Environment.UserName == "xavier"
                ? @"C:\Users\xavie\Documents\lighthouse_test\"
                : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\";

            if (!Directory.Exists(ROOT_PATH))
            {
                MessageBox.Show("Could not locate root directory.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            DevMode = Debugger.IsAttached;
            DatabaseHelper.DatabasePath = $"{ROOT_PATH}manufactureDB.db3";

            string workstation = String.Empty;
            try
            {
                workstation = GetWorkstationLogistics();
            }
            catch
            {
                // Nothing
            }

            if (string.IsNullOrEmpty(workstation))
            {
                Window = new();
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

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                // Need to dispatch to UI thread if performing UI operations
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Debug.WriteLine($"Activated request for {toastArgs.Argument}");
                    if (toastArgs.Argument == "action=viewRequest")
                    {
                        
                        Window.viewModel.UpdateViewCommand.Execute("View Requests");
                    }
                });
            };

            NotificationsManager = new();
            NotificationsManager.Initialise();
            NotificationsManager.DataRefreshTimer.Start();

        }

        //void TestODBC()
        //{

        //    OdbcConnection DbConnection = new(HanaCreds.GetConnString());
        //    DbConnection.Open();

        //    //DataTable tables = DbConnection.GetSchema("Tables");

        //    //foreach (object table in tables.Rows)
        //    //{
        //    //    Debug.WriteLine(table.ToString());
        //    //}

        //    OdbcCommand DbCommand = DbConnection.CreateCommand();
        //    DbCommand.CommandText = $"SELECT \"SlpName\" FROM WIXROYD_UAT2.OSLP";
        //    OdbcDataReader DbReader = DbCommand.ExecuteReader();

        //    int fCount = DbReader.FieldCount;
        //    Debug.Write(":");
        //    for (int i = 0; i < fCount; i++)
        //    {
        //        string fName = DbReader.GetName(i);
        //        Debug.Write(fName + ":");
        //    }
        //    Debug.WriteLine("");

        //    while (DbReader.Read())
        //    {
        //        Debug.Write(":");
        //        for (int i = 0; i < fCount; i++)
        //        {

        //            string col = DbReader.GetString(i) ?? "Error";

        //            Debug.Write(col + ":");
        //        }
        //        Debug.WriteLine("");
        //    }

        //    DbReader.Close();
        //    DbCommand.Dispose();
        //    DbConnection.Close();

        //}

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private static string? GetWorkstationLogistics()
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
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
            ToastNotificationManagerCompat.History.Clear();

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

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            ShowError errorWindow = new() { Error = e };
            errorWindow.NotifyPropertyChanged();

            if (!App.DevMode)
            {
                RecordError(e);
            }

            Window.Hide();
            errorWindow.ShowDialog();
        }

        static void RecordError(System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(e.Exception);
            string filename = DateTime.Now.ToString("s").Replace(':', '_');
            File.WriteAllText($"{ROOT_PATH}/errors/{filename}.json", errorJson);
            string textInfo = $"USER: {App.CurrentUser.UserName}\n" +
                $"COMP: {Environment.MachineName}" +
                $"VIEW: {App.ActiveViewModel}";
            File.WriteAllText($"{ROOT_PATH}/errors/supp_{filename}.txt", textInfo);
        }
    }
}
