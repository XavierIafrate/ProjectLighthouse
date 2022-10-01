using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class MergeRequestToOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public MergeRequestToOrderCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.ApproveRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.ApproveRequest(merge: true);
        }
    }
}
