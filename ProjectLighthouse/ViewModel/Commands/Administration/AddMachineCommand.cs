using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddMachineCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MachineViewModel viewModel;
        public AddMachineCommand(MachineViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not string str) return;
            if (str == "lathe")
            {
                viewModel.AddLathe();
            }
            else if (str == "machine")
            {
                viewModel.AddMachine();
            }
        }
    }
}
