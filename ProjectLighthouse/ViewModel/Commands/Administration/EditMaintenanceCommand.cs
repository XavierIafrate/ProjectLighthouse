using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class EditMaintenanceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MachineViewModel viewModel;
        public EditMaintenanceCommand(MachineViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is MaintenanceEvent maintenanceEvent)
            {
                viewModel.EditMaintenanceEvent(maintenanceEvent);
            }
        }
    }
}
