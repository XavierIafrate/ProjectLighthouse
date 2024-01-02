using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class ApproveRequestCommand : ICommand
    {
        private RequestViewModel viewModel { get; set; }

        public ApproveRequestCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ApproveRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.ApproveRequest();
        }
    }
}
