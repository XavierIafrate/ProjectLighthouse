using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdateRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public UpdateRequestCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.UpdateRequest();
        }
    }
}
