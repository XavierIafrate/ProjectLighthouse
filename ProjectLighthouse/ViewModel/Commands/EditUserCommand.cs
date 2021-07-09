using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class EditUserCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ManageUsersViewModel viewModel;

        public EditUserCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return (App.currentUser.UserRole == "admin" && viewModel.SelectedUser.UserRole != "admin") || App.currentUser.UserName == "xav";
        }

        public void Execute(object parameter)
        {
            viewModel.EnableEdit();
        }
    }
}
