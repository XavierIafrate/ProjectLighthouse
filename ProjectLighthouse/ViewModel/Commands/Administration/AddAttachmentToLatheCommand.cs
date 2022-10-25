using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class AddAttachmentToLatheCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private LatheViewModel viewModel;
        public AddAttachmentToLatheCommand(LatheViewModel vm)
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
