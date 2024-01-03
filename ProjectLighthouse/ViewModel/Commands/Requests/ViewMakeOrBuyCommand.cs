using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class ViewMakeOrBuyCommand : ICommand
    {
        private RequestViewModel viewModel;
        public ViewMakeOrBuyCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ShowMakeOrBuy();
        }
    }
}
