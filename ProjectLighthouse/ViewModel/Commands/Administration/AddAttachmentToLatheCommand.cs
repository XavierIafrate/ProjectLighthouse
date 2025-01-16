using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddAttachmentToLatheCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MachineViewModel viewModel;
        public AddAttachmentToLatheCommand(MachineViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddAttachment();
        }
    }
}
