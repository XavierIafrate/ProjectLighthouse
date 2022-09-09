using System;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class SendRuntimeReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private AnalyticsViewModel viewModel;

        public SendRuntimeReportCommand(AnalyticsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.UserName == "xav";
        }

        public void Execute(object parameter)
        {
            if (parameter.ToString() == "all")
            {
                viewModel.SendRuntimeReport(test: false);

            }
            else if (parameter.ToString() == "test")
            {
                viewModel.SendRuntimeReport(test: true);
            }
            else
            {
                MessageBox.Show("parameter failed");
            }
        }
    }
}
