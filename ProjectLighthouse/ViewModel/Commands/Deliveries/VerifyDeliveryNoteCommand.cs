using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Deliveries
{
    public class VerifyDeliveryNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DeliveriesViewModel viewModel;

        public VerifyDeliveryNoteCommand(DeliveriesViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.CreateDelivery);
        }

        public void Execute(object parameter)
        {
            viewModel.VerifySelectedDeliveryNote();
        }
    }
}
