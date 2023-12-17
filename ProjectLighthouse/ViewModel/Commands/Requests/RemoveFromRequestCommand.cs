using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class RemoveFromRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public RemoveFromRequestCommand(RequestViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is RequestItem i)
            {
                viewModel.RemoveFromRequest(i);
            }
        }
    }
}
