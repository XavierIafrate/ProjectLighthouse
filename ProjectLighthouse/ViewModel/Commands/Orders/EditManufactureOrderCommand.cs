using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Orders;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class EditManufactureOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private OrderViewModel viewModel;

        public EditManufactureOrderCommand(OrderViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.EditLMO();
        }
    }
}
