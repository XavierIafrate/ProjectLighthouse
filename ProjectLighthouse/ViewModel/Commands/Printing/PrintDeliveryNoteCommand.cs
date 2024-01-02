using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Printing
{
    public class PrintDeliveryNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public DeliveriesViewModel viewModel { get; set; }

        public PrintDeliveryNoteCommand(DeliveriesViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.SelectedDeliveryNote != null && viewModel.FilteredDeliveryItems != null;
        }

        public void Execute(object parameter)
        {
            viewModel.PrintDeliveryNotePDF();
        }
    }
}
