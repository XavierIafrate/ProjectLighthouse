using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class FirebaseSyncCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ScheduleViewModel viewModel;
        public FirebaseSyncCommand(ScheduleViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.SyncWithFirebase();
        }
    }
}
