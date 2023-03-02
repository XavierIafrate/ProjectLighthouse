using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class AddProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ProductManagerViewModel viewModel;

        public AddProductCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Product p)
            {
                viewModel.CreateProduct(p);
                return;
            }

            viewModel.CreateProduct();
        }
    }
}
