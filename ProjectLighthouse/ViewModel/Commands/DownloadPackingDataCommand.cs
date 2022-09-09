using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class DownloadPackingDataCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AnalyticsViewModel viewModel;

        public DownloadPackingDataCommand(AnalyticsViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.UserName == "xav";
        }

        public void Execute(object parameter)
        {
            viewModel.DownloadPackingData();
        }
    }
}
