using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
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
