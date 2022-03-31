using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
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
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AutoSchedule();
        }
    }
}
