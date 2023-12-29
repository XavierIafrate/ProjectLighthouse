using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddProductGroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ProductManagerViewModel viewModel;

        public AddProductGroupCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.CreateProducts);
        }

        public void Execute(object parameter)
        {
            if (parameter is ProductGroup group)
            {
                viewModel.AddProductGroup(group);
                return;
            }

            viewModel.AddProductGroup(null);
        }
    }
}
