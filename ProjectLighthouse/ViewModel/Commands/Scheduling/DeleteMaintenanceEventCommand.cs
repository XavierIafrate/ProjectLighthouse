using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class DeleteMaintenanceEventCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ScheduleViewModel viewModel;

        public DeleteMaintenanceEventCommand(ScheduleViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ConfigureMaintenance);
        }

        public void Execute(object parameter)
        {
            if (parameter is not MachineService service) return;
            viewModel.CancelService(service);
        }
    }
}
