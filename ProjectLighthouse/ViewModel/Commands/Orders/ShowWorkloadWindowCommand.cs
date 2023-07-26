using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Orders
{
    public class ShowWorkloadWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private OrderViewModel viewModel;

        public ShowWorkloadWindowCommand(OrderViewModel vm)
        {
            this.viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ShowWorkload();
        }
    }
}
