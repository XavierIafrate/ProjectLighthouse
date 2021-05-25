using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class AutoScheduleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ScheduleViewModel viewModel;

        public AutoScheduleCommand(ScheduleViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            var tab = viewModel.SelectedTab;
            return tab.Orders.Count > 1;
        }

        public void Execute(object parameter)
        {
            viewModel.AutoSchedule();
        }
    }
}
