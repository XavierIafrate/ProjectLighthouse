using ProjectLighthouse.Model;
using System.Windows;

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
}
