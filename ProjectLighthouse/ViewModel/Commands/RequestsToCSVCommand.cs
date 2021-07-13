using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
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
            return App.CurrentUser.UserRole == "Purchasing" || App.CurrentUser.UserRole == "Scheduling" || App.CurrentUser.UserRole == "admin";
        }

        public void Execute(object parameter)
        {
            viewModel.ExportRequestsToCSV();
        }
    }
}
