using LighthouseMonitoring.Model;
using LighthouseMonitoring.ViewModel;
using ProjectLighthouse;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Threading;

namespace LighthouseMonitoring
{
    public partial class App : Application
    {
        public static MainViewModel ViewModel { get; set; }
        public const string DEBUG_ROOT = ApplicationRootPaths.DEBUG_ROOT;
        public const string DEBUG_DB_NAME = ApplicationRootPaths.DEBUG_DB_NAME;

        public static MonitoringSystem MonitoringSystem { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        public App()
        {
            EstablishDatabaseContext();

            MonitoringSystem = new();
            MonitoringSystem.Initialise();

            MainWindow window = new();
            ViewModel = new(window);
        }

        private void EstablishDatabaseContext()
        {

            DatabaseHelper.DatabasePath = $"{ApplicationRootPaths.DEBUG_ROOT}{ApplicationRootPaths.DEBUG_DB_NAME}";
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            // TODO

            //RecordError(e);


            //ShowError errorWindow = new() { Error = e };
            //errorWindow.NotifyPropertyChanged();


            //Window.Hide();
            //errorWindow.ShowDialog();
        }
    }
}
