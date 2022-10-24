using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.UserManagement
{
    public class GrantPermissionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ManageUsersViewModel viewModel;

        public GrantPermissionCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddPermission();
        }
    }
}
