using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddTurnedProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ProductManagerViewModel viewModel;
        public AddTurnedProductCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }


        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddTurnedProduct();
        }
    }
}
