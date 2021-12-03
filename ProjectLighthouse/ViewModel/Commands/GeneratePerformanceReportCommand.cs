using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class GeneratePerformanceReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AnalyticsViewModel viewModel;

        public GeneratePerformanceReportCommand(AnalyticsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.GenerateReport();
        }
    }
}
