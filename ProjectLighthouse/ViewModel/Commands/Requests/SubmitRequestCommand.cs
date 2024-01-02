using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class SubmitRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public SubmitRequestCommand(RequestViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.RaiseRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.SubmitRequest();
        }
    }
}
