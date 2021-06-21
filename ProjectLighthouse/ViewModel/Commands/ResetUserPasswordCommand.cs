using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
