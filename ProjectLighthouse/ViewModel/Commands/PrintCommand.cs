using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    class PrintCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public OrderViewModel viewModel;

        public PrintCommand(OrderViewModel vm)
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
