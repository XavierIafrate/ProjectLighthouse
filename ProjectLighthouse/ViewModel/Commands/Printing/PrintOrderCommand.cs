using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Orders;

namespace ProjectLighthouse.ViewModel.Commands.Printing
{
    public class PrintOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public OrderViewModel viewModel;

        public PrintOrderCommand(OrderViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PrintSelectedOrder();
        }
    }
}
