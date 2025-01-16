using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Deliveries
{
    public class EditDeliveryItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private DeliveriesViewModel viewModel;
        public EditDeliveryItemCommand(DeliveriesViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.EditDelivery);
        }

        public void Execute(object parameter)
        {
            viewModel.EditDeliveryItem();
        }
    }
}
