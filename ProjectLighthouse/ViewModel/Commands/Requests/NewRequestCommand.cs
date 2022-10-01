using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class NewRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NewRequestViewModel viewModel;

        public NewRequestCommand(NewRequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.RaiseRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.SubmitRequest();
        }
    }
}
