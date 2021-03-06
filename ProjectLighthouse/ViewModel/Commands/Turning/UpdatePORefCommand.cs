using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdatePORefCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public RequestViewModel viewModel;

        public UpdatePORefCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.UpdateOrderPurchaseRef();
        }
    }
}
