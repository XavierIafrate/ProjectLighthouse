using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.UserManagement
{
    public class ResetUserPasswordCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ManageUsersViewModel viewModel { get; set; }

        public ResetUserPasswordCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role == UserRole.Administrator;
        }

        public void Execute(object parameter)
        {
            viewModel.ResetPassword();
        }
    }
}
