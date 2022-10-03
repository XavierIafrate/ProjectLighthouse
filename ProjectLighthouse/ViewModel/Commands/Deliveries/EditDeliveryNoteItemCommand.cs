using System;
using System.Windows.Input;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Orders;

namespace ProjectLighthouse.ViewModel.Commands.Deliveries
{
    public class EditDeliveryNoteItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public DeliveriesViewModel viewModel;

        public EditDeliveryNoteItemCommand(DeliveriesViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.EditDelivery);
        }

        public void Execute(object parameter)
        {
            if (parameter is int id)
            {
                viewModel.EditItem(id);
            }
        }
    }
}
