using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class DeleteUserCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ManageUsersViewModel viewModel;

        public DeleteUserCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.UserRole == "admin";
        }

        public void Execute(object parameter)
        {
            viewModel.DeleteUser();
        }
    }
}
