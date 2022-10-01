using System;
using System.Windows.Input;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Requests;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class RequestsToCSVCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public RequestsToCSVCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= UserRole.Purchasing;
        }

        public void Execute(object parameter)
        {
            viewModel.ExportRequestsToCSV();
        }
    }
}
