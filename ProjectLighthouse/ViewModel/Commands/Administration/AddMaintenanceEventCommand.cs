using ProjectLighthouse;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class AddMaintenanceEventCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private LatheViewModel viewModel;
        public AddMaintenanceEventCommand(LatheViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(ProjectLighthouse.Model.Core.PermissionType.ConfigureMaintenance);
        }

        public void Execute(object parameter)
        {
            viewModel.AddMaintenanceEvent();
        }
    }
}
