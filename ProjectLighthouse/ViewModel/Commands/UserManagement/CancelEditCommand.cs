using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.UserManagement
{
    public class CancelEditCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        ManageUsersViewModel viewModel;

        public CancelEditCommand(ManageUsersViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.CancelEdit();
        }
    }
}
