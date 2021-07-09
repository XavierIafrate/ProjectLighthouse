using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
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
            return App.currentUser.UserRole == "admin";
        }

        public void Execute(object parameter)
        {
            viewModel.ResetPassword();
        }
    }
}
