using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.UserManagement
{
    public class RevokePermissionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ManageUsersViewModel viewModel;
        public RevokePermissionCommand(ManageUsersViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.RevokePermission();
        }
    }
}
