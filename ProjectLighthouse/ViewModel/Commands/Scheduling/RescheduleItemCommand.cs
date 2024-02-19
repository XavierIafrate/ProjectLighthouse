using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class RescheduleItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private NewScheduleViewModel viewModel;

        public RescheduleItemCommand(NewScheduleViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= Model.Administration.UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            if (parameter is not RescheduleInformation info) return;

            viewModel.Reschedule(info);
        }
    }
}
