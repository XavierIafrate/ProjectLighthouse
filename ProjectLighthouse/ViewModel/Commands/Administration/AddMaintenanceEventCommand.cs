using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddMaintenanceEventCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MachineViewModel viewModel;
        public AddMaintenanceEventCommand(MachineViewModel vm)
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
