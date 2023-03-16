using ProjectLighthouse.ViewModel.Programs;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Programs
{
    public class OpenProgramCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ProgramManagerViewModel viewModel;

        public OpenProgramCommand(ProgramManagerViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.OpenProgram();
        }
    }
}
