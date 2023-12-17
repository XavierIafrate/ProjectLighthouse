using System;
using System.Windows.Input;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.ViewModel.Requests;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class EditItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RequestViewModel viewModel;

        public EditItemCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateProducts);
        }

        public void Execute(object parameter)
        {
            if (parameter is not RequestItem item) return;
            viewModel.EditItem(item);
        }
    }
}
