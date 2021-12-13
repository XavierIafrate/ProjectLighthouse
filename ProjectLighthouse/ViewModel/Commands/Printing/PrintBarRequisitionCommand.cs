using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class PrintBarRequisitionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private BarStockViewModel viewModel { get; set; }

        public PrintBarRequisitionCommand(BarStockViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PrintRequisition();
        }
    }
}
