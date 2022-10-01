using System;
using System.Windows.Input;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Orders;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class AddResearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ScheduleViewModel _viewModel;

        public AddResearchCommand(ScheduleViewModel vm)
        {
            _viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            _viewModel.AddResearch();
        }
    }
}
