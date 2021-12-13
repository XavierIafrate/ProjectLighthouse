using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class DebugViewModel : BaseViewModel
    {
        public static void Test()
        {
            _ = MessageBox.Show("Test");
        }

        public static void ExportErrorStates()
        {

        }
    }
}
