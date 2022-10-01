using System.Windows;

namespace ProjectLighthouse.ViewModel.Core
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
