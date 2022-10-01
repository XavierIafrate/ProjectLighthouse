using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Core;

namespace ProjectLighthouse.ViewModel.Commands.Notifications
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
