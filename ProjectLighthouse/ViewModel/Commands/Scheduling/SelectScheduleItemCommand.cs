using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class SelectScheduleItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private NewScheduleViewModel NewScheduleViewModel;

        public SelectScheduleItemCommand(NewScheduleViewModel viewModel)
        {
            this.NewScheduleViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is null)
            {
                NewScheduleViewModel.SelectItem(null);
            }

            if (parameter is not ScheduleItem item) return;
            NewScheduleViewModel.SelectItem(item);
        }
    }
}
