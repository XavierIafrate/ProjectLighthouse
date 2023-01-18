using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
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
            //if (parameter.ToString() == "all")
            //{
            //    viewModel.CAlci(test: false);

            //}
        }
    }
}
