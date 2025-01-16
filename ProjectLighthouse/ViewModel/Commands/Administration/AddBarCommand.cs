using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
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
            if(parameter is BarStock barToEdit)
            {
                viewModel.EditBar(barToEdit);
            }
            else
            {
                viewModel.AddNewBar();
            }
        }
    }
}
