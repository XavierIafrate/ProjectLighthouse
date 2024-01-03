using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Printing
{
    public class PrintScheduleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ScheduleViewModel viewModel;

        public PrintScheduleCommand(ScheduleViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PrintSchedule();
        }
    }
}
