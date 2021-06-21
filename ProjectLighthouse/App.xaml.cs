using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Threading;

namespace ProjectLighthouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User currentUser { get; set; }
        public static string ROOT_PATH { get; set; }
    }

    //public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    //{
    //    //log.Fatal("An unexpected application exception occurred", args.Exception);

    //    MessageBox.Show($"An unexpected exception has occurred. {args.Exception}");

    //    // Prevent default unhandled exception processing
    //    args.Handled = true;

    //    Environment.Exit(0);
    //}
}
