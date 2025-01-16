using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class RemoveAttachmentFromLatheCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MachineViewModel viewModel;
        public RemoveAttachmentFromLatheCommand(MachineViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Attachment attachment)
            {
                viewModel.RemoveAttachment(attachment);
            }
        }
    }
}
