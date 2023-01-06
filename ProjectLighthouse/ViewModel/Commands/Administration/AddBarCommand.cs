using ProjectLighthouse;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class AddBarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private BarStockViewModel viewModel;

        public AddBarCommand(BarStockViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= ProjectLighthouse.Model.Administration.UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            viewModel.AddNewBar();
        }
    }
}
