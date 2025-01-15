using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class ImportTargetStockCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public DatabaseManagerViewModel viewModel;
        public ImportTargetStockCommand(DatabaseManagerViewModel vm)
        {
            this.viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateProducts);
        }

        public void Execute(object parameter)
        {
            viewModel.RunImportTriggerLevels();
        }
    }
}
