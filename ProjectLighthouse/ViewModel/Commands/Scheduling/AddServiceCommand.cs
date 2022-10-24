using System;
using System.Windows.Input;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Orders;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class AddServiceCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ScheduleViewModel _viewModel;

        public AddServiceCommand(ScheduleViewModel vm)
        {
            _viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ApproveRequest);
        }

        public void Execute(object parameter)
        {
            _viewModel.AddMaintenance();
        }
    }
}
