using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdateItemOnScheduleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ScheduleViewModel viewModel;

        public UpdateItemOnScheduleCommand(ScheduleViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin");
        }

        public void Execute(object parameter)
        {
            viewModel.UpdateOrder(parameter as LatheManufactureOrder);
        }
    }
}
