using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Orders
{
    public class GetProgramPlannerCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private OrderViewModel viewModel;
        public GetProgramPlannerCommand(OrderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.CreateProgramPlanner();
        }
    }
}
