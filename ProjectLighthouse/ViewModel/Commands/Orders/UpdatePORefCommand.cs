using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Requests;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class UpdatePORefCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public RequestViewModel viewModel;

        public UpdatePORefCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateOrder);
        }

        public void Execute(object parameter)
        {
            viewModel.UpdateOrderPurchaseRef();
        }
    }
}
