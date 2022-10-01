using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Deliveries
{
    public class NewDeliveryCommand : ICommand
    {
        private DeliveriesViewModel viewModel { get; set; }

        public NewDeliveryCommand(DeliveriesViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.CreateDelivery);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            viewModel.CreateNewDelivery();
        }
    }
}
