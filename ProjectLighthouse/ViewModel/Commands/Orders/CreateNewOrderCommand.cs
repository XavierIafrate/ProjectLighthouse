using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class CreateNewOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NewOrderViewModel viewModel;

        public CreateNewOrderCommand(NewOrderViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ApproveRequest);
        }

        public void Execute(object parameter)
        {
            viewModel.CreateNewOrder();
        }
    }
}
