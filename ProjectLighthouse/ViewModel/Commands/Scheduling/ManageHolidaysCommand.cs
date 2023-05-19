using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class ManageHolidaysCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ScheduleViewModel viewModel;

        public ManageHolidaysCommand(ScheduleViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            viewModel.EditHolidays();
        }
    }
}
