using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Quality;

namespace ProjectLighthouse.ViewModel.Commands.Quality
{
    public class AddQualityCheckCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private QualityCheckViewModel viewModel;

        public AddQualityCheckCommand(QualityCheckViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddQualityCheck();
        }
    }
}
