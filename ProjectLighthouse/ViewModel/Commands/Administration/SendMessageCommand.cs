using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Quality;
using ProjectLighthouse.ViewModel.Requests;
using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class SendMessageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private BaseViewModel viewModel;

        public SendMessageCommand(BaseViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (viewModel is RequestViewModel rvm)
            {
                rvm.SendMessage();
            }
            else if (viewModel is QualityCheckViewModel qcvm)
            {
                qcvm.SendMessage();
            }
            else if (viewModel is ResearchViewModel devvm)
            {
                devvm.SendMessage();
            }
        }
    }
}
