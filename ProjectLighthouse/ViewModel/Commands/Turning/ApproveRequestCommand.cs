using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class ApproveRequestCommand : ICommand
    {
        private RequestViewModel viewModel { get; set; }

        public ApproveRequestCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ApproveRequest();
        }
    }
}
