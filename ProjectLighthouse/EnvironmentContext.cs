using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.IO;
using System.Windows;

namespace ProjectLighthouse
{
    public class EnvironmentContext
    {
        public static bool Setup()
        {
#if DEBUG
            App.ROOT_PATH = ApplicationRootPaths.DEBUG_ROOT;
            DatabaseHelper.DatabasePath = $"{ApplicationRootPaths.DEBUG_ROOT}{ApplicationRootPaths.DEBUG_DB_NAME}";
            App.DevMode = true;
#elif DEMO
            App.DemoMode = true;
            App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataFolder = Path.Combine(localAppData, "lighthouse");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            string filePath = Path.Combine(appDataFolder, "root.txt");
            DemoMode demoModeWindow;

            try
            {
                string targetRoot = File.ReadAllText(filePath);

                if (App.ValidateRootDirectory(targetRoot, demo:true))
                {
                    App.ROOT_PATH = targetRoot;
                    DatabaseHelper.DatabasePath = $"{App.ROOT_PATH}{ApplicationRootPaths.DEMO_DB_NAME}";
                    return true;
                }
            }
            catch
            {

            }

            demoModeWindow = new();

            demoModeWindow.ShowDialog();

            if (demoModeWindow.failed)
            {
                return false;
            }

            App.ROOT_PATH = demoModeWindow.rootDirectory;
            DatabaseHelper.DatabasePath = $"{App.ROOT_PATH}{ApplicationRootPaths.DEMO_DB_NAME}";

            File.WriteAllText(filePath, App.ROOT_PATH);

#else
            App.ROOT_PATH = ApplicationRootPaths.RELEASE_ROOT;
            DatabaseHelper.DatabasePath = $"{ApplicationRootPaths.RELEASE_ROOT}{ApplicationRootPaths.RELEASE_DB_NAME}";
            //DatabaseHelper.DatabasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{ApplicationRootPaths.RELEASE_DB_NAME}";
            App.DevMode = false;
#endif

            if (!Directory.Exists(App.ROOT_PATH) || !File.Exists(DatabaseHelper.DatabasePath))
            {
                return false;
            }

            return true;
        }
    }
}
