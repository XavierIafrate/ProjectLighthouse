using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.View;
using ProjectLighthouse.View.Core;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public static bool DevMode { get; set; }
        public static bool DemoMode { get; set; }
        public static Constants Constants { get; set; }

        private static MainWindow Window;
        public static MainViewModel MainViewModel { get; set; }
        public static NotificationManager NotificationsManager { get; set; }
        public static string AppDataDirectory { get; set; }
        public static List<StandardFit> StandardFits { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            if (!EnvironmentContext.Setup())
            {
                SetupFailedWindow window = new("Something went wrong while setting up the environment.", App.ROOT_PATH);
                window.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            //if (!ValidateRootDirectory(App.ROOT_PATH, App.DemoMode))
            //{
            //    string message = $"{App.ROOT_PATH} failed validation checks:";
            //    foreach (string str in GetRootDirectoryErrors(App.ROOT_PATH, App.DemoMode))
            //    {
            //        message += $"{Environment.NewLine}{str}";
            //    }

            //    SetupFailedWindow window = new("Invalid Network Root.", message);
            //    window.ShowDialog();
            //    Application.Current.Shutdown();
            //}

            try
            {
                
                string constantsJson = File.ReadAllText(App.ROOT_PATH + "config.json");
                Constants = Newtonsoft.Json.JsonConvert.DeserializeObject<Constants>(constantsJson);
            }
            catch (Exception ex)
            {
                SetupFailedWindow window = new("Cannot load config file.", $"{ex.Message}");
                window.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            try
            {
                _ = DatabaseHelper.Read<User>(throwErrs: true).ToList();
            }
            catch (SQLite.SQLiteException ex)
            {
                SetupFailedWindow window = new("The database cannot be read.", $"{ex.Source}: {ex.Result}");
                window.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            if (!VersionIsPermitted())
            {
                SetupFailedWindow window = new("You are trying to load an outdated version of Lighthouse.", $"Loading v{GetAppVersion()}");
                window.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            MainViewModel VM = LoadMain();
            Task.Run(() => EnsureAppData());

            bool userLoggedIn = VM.LoginRoutine();

            if (!userLoggedIn)
            {
                Application.Current.Shutdown();
                return;
            }

            Window.Show();
            Window.AddVersionNumber();
#if DEMO
            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            App.Constants.BaseProgramPath = $"{App.ROOT_PATH}lib\\programs\\";
#endif

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

            LumenManager.Initialise();
        }

        private static bool VersionIsPermitted()
        {
            string[] allowed_versions;

            try
            {
                allowed_versions = File.ReadAllLines($"{App.ROOT_PATH}valid_versions.txt");
            }
            catch
            {
                return false;
            }

            if (!allowed_versions.Any(x => x == GetAppVersion()))
            {
                return false;
            }

            return true;
        }

        private static string GetAppVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            return versionInfo.FileVersion;
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
            Window.ViewModel = VM;

            return VM;
        }

        private static void StartNotificationsManager()
        {
            NotificationsManager = new();
            NotificationsManager.Initialise();
            NotificationsManager.DataRefreshTimer.Start();
            NotificationsManager.CheckForNotifications(true);
        }

        private static void EnsureAppData()
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


        public static void MoveToLocalAppData(string path)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string specificFolder = Path.Combine(folder, "Lighthouse");
            if (!Directory.Exists(specificFolder))
            {
                Directory.CreateDirectory(specificFolder);
            }

            string copyTo = $@"{specificFolder}\lib\";

            try
            {
                File.Copy(path, copyTo + Path.GetFileName(path), true);
            }
            catch
            {
                throw;
            }
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

            if (e.Exception is NotImplementedException)
            {
                MessageBox.Show("The action executed has not been implemented yet.", "Exception", MessageBoxButton.OK, MessageBoxImage.Stop);
                e.Handled = true;
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
            App.Current.Shutdown();
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

        public static bool ValidateRootDirectory(string rootDirectory, bool demo = false)
        {
            string[] errors = GetRootDirectoryErrors(rootDirectory, demo);
            return errors.Length == 0;
        }

        public static string[] GetRootDirectoryErrors(string rootDirectory, bool demo = false)
        {
            string dbFile;
            List<string> errors = new();

            if (demo)
            {
                dbFile = $"{rootDirectory}{ApplicationRootPaths.DEMO_DB_NAME}";
            }
            else
            {
                dbFile = DatabaseHelper.DatabasePath;
            }

            if (!File.Exists(dbFile))
            {
                errors.Add($"File '{dbFile}' does not exist.");
            }

            string[] directories = new string[]
            {
                "Calibration",
                "errors",
                "lib",
                "lib\\gen",
                "lib\\logs",
                "lib\\checksheets",
                "lib\\pcom",
                "lib\\renders",
                "print",
            };

            foreach (string directory in directories)
            {
                if (!Directory.Exists($"{rootDirectory}{directory}"))
                {
                    errors.Add($"Directory '{rootDirectory}{directory}' does not exist.");
                }
            }

            if (demo)
            {
                if (!Directory.Exists($"{rootDirectory}lib\\programs"))
                {
                    errors.Add($"Directory '{rootDirectory}lib\\programs' does not exist.");
                }
            }

            string[] files = new string[]
            {
                "config.json",
                "fits.txt",
                "lathes.json",
                "Lighthouse_dark.png",
                "Lighthouse_Mono_L_Embedded.png",
                "valid_versions.txt",
                "lib\\holidays.json",
            };

            foreach (string file in files)
            {
                if (!File.Exists($"{rootDirectory}{file}"))
                {
                    errors.Add($"File '{rootDirectory}{file}' does not exist.");

                }
            }

            return errors.ToArray();
        }
    }
}
