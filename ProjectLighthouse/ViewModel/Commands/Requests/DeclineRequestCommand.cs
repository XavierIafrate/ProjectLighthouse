using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Requests;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class DeclineRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RequestViewModel viewModel { get; set; }

        public DeclineRequestCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ApproveRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.DeclineRequest();
        }
    }
}
