using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
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

            MainWindow Window = new();
            MainViewModel VM = new() { MainWindow = Window };

            Window.DataContext = VM;
            Window.viewModel = VM;

            VM.LoginRoutine();
            Window.Show();

            Window.AddVersionNumber();
            _ = Window.CheckForUpdates();
        }
    }
}
