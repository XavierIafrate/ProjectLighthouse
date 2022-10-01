using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Quality;

namespace ProjectLighthouse.ViewModel.Commands.Calibration
{
    public class GenerateReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        CalibrationViewModel viewModel;

        public GenerateReportCommand(CalibrationViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.CreateReport();
        }
    }
}
