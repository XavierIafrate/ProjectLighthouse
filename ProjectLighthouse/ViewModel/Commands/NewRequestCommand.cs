using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class NewRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NewRequestViewModel viewModel;

        public NewRequestCommand(NewRequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.currentUser.CanRaiseRequest;
        }

        public void Execute(object parameter)
        {
            viewModel.SubmitRequest();
        }
    }
}
