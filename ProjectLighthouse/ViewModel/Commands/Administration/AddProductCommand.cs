using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
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
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.CreateProducts);
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
