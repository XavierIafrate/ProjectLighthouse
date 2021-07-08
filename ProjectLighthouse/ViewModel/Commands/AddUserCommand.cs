using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class AddUserCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ManageUsersViewModel viewModel { get; set; }

        public AddUserCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.currentUser.UserRole == "admin";
        }

        public void Execute(object parameter)
        {
            viewModel.CreateNewUser();
        }
    }
}
