using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class CreateNewOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private OrderViewModel viewModel;

        public CreateNewOrderCommand(OrderViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= Model.UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            viewModel.CreateNewOrder();
        }
    }
}
