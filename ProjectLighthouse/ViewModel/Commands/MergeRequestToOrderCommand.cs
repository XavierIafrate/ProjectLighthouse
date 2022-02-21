using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
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
            return App.CurrentUser.CanApproveRequests;
        }

        public void Execute(object parameter)
        {
            viewModel.ApproveRequest(merge: true);
        }
    }
}
