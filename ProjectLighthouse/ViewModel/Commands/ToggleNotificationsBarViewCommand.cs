using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class ToggleNotificationsBarViewCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainViewModel viewModel;

        public ToggleNotificationsBarViewCommand(MainViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ToggleNotificationsBarVisibility();
        }
    }
}
