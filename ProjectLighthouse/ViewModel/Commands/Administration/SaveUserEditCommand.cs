using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class SaveUserEditCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ManageUsersViewModel viewModel;

        public SaveUserEditCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.SaveEdit();
        }
    }
}
