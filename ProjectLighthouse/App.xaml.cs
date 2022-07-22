using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
        public static string AppDataDirectory { get; set; }
        public static List<string> AppTrace = new();

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

            EnsureAppData();

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

                bool userLoggedIn = VM.LoginRoutine();

                if (!userLoggedIn)
                {
                    Application.Current.Shutdown();
                }
                Window.Show();

                Window.AddVersionNumber();
            }
            else
            {
                LogisticsKioskWindow window = new()
                {
                    WorkStation = workstation
                };
                window.LoginRoutine();
            }


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

            NotificationsManager = new();
            NotificationsManager.Initialise();
            NotificationsManager.DataRefreshTimer.Start();
            NotificationsManager.CheckForNotifications(true);
        }


        //private void CheckForAppUpdates()
        //{
        //    UpdateCheckInfo info = null;

        //    if (ApplicationDeployment.IsNetworkDeployed)
        //    {
        //        ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

        //        try
        //        {
        //            info = ad.CheckForDetailedUpdate();

        //        }
        //        catch (DeploymentDownloadException dde)
        //        {
        //            MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
        //            return;
        //        }
        //        catch (InvalidDeploymentException ide)
        //        {
        //            MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
        //            return;
        //        }
        //        catch (InvalidOperationException ioe)
        //        {
        //            MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
        //            return;
        //        }

        //        if (info.UpdateAvailable)
        //        {
        //            Boolean doUpdate = true;

        //            if (!info.IsUpdateRequired)
        //            {
        //                DialogResult dr = MessageBox.Show("An update is available. Would you like to update the application now?", "Update Available", MessageBoxButtons.OKCancel);
        //                if (!(DialogResult.OK == dr))
        //                {
        //                    doUpdate = false;
        //                }
        //            }
        //            else
        //            {
        //                // Display a message that the app MUST reboot. Display the minimum required version.
        //                MessageBox.Show("This application has detected a mandatory update from your current " +
        //                    "version to version " + info.MinimumRequiredVersion.ToString() +
        //                    ". The application will now install the update and restart.",
        //                    "Update Available", MessageBoxButtons.OK,
        //                    MessageBoxIcon.Information);
        //            }

        //            if (doUpdate)
        //            {
        //                try
        //                {
        //                    ad.Update();
        //                    MessageBox.Show("The application has been upgraded, and will now restart.");
        //                    Application.Restart();
        //                }
        //                catch (DeploymentDownloadException dde)
        //                {
        //                    MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
        //                    return;
        //                }
        //            }
        //        }
        //    }
        //}
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
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
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
