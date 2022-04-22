using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
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
            return App.CurrentUser.CanRaiseDelivery;
        }

        public void Execute(object parameter)
        {
            viewModel.VerifySelectedDeliveryNote();
        }
    }
}
