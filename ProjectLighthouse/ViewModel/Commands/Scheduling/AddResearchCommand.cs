using System;
using System.Windows.Input;

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
            return App.CurrentUser.UserRole is "admin" or "Scheduling";
        }

        public void Execute(object parameter)
        {
            _viewModel.AddResearch();
        }
    }
}
