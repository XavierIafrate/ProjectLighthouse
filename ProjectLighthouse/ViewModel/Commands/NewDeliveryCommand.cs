using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
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
            return App.currentUser.CanRaiseDelivery;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            viewModel.CreateNewDelivery();
        }
    }
}
