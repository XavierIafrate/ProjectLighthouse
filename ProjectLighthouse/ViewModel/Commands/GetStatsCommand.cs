using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class GetStatsCommand : ICommand
    {
        private MachineStatsViewModel viewModel;

        public GetStatsCommand(MachineStatsViewModel vm)
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
            viewModel.getStats();
        }
    }
}
