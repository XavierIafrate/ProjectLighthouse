using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class EditMachineCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MachineViewModel viewModel;
        public EditMachineCommand(MachineViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Lathe lathe)
            {
                viewModel.EditLathe(lathe);
            }
            else if (parameter is Machine machine)
            {
                viewModel.EditMachine(machine);
            }
        }
    }
}
