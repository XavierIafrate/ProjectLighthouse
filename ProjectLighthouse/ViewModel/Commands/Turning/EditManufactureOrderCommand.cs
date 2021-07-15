using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    class EditManufactureOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private OrderViewModel viewModel;

        public EditManufactureOrderCommand(OrderViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanUpdateLMOs;
        }

        public void Execute(object parameter)
        {
            viewModel.EditLMO();
        }
    }
}
