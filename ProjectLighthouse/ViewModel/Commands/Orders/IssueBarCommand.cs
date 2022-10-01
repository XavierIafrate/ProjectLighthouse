using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class IssueBarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private BarStockViewModel viewModel;

        public IssueBarCommand(BarStockViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.IssueBar);
        }

        public void Execute(object parameter)
        {
            viewModel.CreateBarIssue();
        }
    }
}
