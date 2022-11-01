using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Foundation.Collections;

namespace ProjectLighthouse
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static Login Login { get; set; }
        public static string ROOT_PATH { get; set; }
        public static string ActiveViewModel { get; set; }
        public static bool DevMode { get; set; }
        private static MainWindow Window;
        public static MainViewModel MainViewModel { get; set; }
        public static NotificationManager NotificationsManager { get; set; }
        public static string AppDataDirectory { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            base.OnStartup(e);
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            if (!EnvironmentContext.Setup())
            {
                MessageBox.Show("Something wen't wrong while setting up the environment, Lighthouse cannot start.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            MainViewModel VM = LoadMain();

            bool userLoggedIn = VM.LoginRoutine();

            if (!userLoggedIn)
            {
                Application.Current.Shutdown();
                return;
            }

            Window.Show();
            Window.AddVersionNumber();
            Task.Run(() => EnsureAppData());

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                ValueSet userInput = toastArgs.UserInput;

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Debug.WriteLine($"Activated request for {toastArgs.Argument}");
                    NotificationsManager.ParseToastArgs(toastArgs.Argument);
                });
            };

            Task.Run(() => StartNotificationsManager());
        }

        private static MainViewModel LoadMain()
        {
            Window = new();
            MainViewModel VM = new()
            {
                MainWindow = Window
            };

            MainViewModel = VM;

            Window.DataContext = VM;
            Window.viewModel = VM;

            return VM;
        }


        private void StartNotificationsManager()
        {
            NotificationsManager = new();
            NotificationsManager.Initialise();
            NotificationsManager.DataRefreshTimer.Start();
            NotificationsManager.CheckForNotifications(true);
        }

        private void EnsureAppData()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string specificFolder = Path.Combine(folder, "Lighthouse");
            AppDataDirectory = $@"{specificFolder}\";
            if (!Directory.Exists(specificFolder))
            {
                Directory.CreateDirectory(specificFolder);
            }

            string copyFrom = $@"{ROOT_PATH}lib\";
            string copyTo = $@"{specificFolder}\lib\";

            CopyFilesRecursively(copyFrom, copyTo);
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        void TestODBC()
        {

            OdbcConnection DbConnection = new(HanaCreds.GetConnString());
            DbConnection.Open();

            //DataTable tables = DbConnection.GetSchema("Tables");

            //foreach (object table in tables.Rows)
            //{
            //    Debug.WriteLine(table.ToString());
            //}

            OdbcCommand DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = $"SELECT T0.\"ItemCode\", T0.\"ItemName\", T0.\"CardCode\", T0.\"U_V33_Automotion\", T0.\"OnHand\" FROM WIXROYD_UAT2.OITM T0 WHERE T0.\"U_V33_Automotion\" IS NOT NULL";
            OdbcDataReader DbReader = DbCommand.ExecuteReader();

            int fCount = DbReader.FieldCount;
            Debug.Write(":");
            for (int i = 0; i < fCount; i++)
            {
                string fName = DbReader.GetName(i);
                Debug.Write(fName + ":");
            }
            Debug.WriteLine("");

            while (DbReader.Read())
            {
                Debug.Write(":");
                for (int i = 0; i < fCount; i++)
                {

                    string col = DbReader.GetString(i) ?? "Error";

                    Debug.Write(col + ":");
                }
                Debug.WriteLine("");
            }

            DbReader.Close();
            DbCommand.Dispose();
            DbConnection.Close();

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
            if (e.Handled || Window == null)
            {
                return;
            }
            if (!App.DevMode && App.CurrentUser != null)
            {
                RecordError(e);
            }

            ShowError errorWindow = new() { Error = e };
            errorWindow.NotifyPropertyChanged();


            Window.Hide();
            errorWindow.ShowDialog();
        }

        static void RecordError(System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorJson = Newtonsoft.Json.JsonConvert.SerializeObject(e.Exception);
            string filename = DateTime.Now.ToString("s").Replace(':', '_');
            File.WriteAllText($"{ROOT_PATH}/errors/{filename}.json", errorJson);
            string textInfo = $"USER: {App.CurrentUser.UserName}\n" +
                $"COMP: {Environment.MachineName}\n" +
                $"VIEW: {App.ActiveViewModel}";
            File.WriteAllText($"{ROOT_PATH}/errors/supp_{filename}.txt", textInfo);
        }
    }
}
