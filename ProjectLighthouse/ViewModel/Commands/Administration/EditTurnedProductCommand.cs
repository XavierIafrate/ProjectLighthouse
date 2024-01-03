using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class EditTurnedProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ProductManagerViewModel viewModel;

        public EditTurnedProductCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateProducts);

        }

        public void Execute(object parameter)
        {
            if (parameter is not int id)
            {
                return;
            }

            viewModel.EditTurnedProduct(id);
        }
    }
}
