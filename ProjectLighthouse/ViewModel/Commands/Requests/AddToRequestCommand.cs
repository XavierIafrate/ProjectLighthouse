using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class AddToRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public AddToRequestCommand(RequestViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TurnedProduct p)
            {
                viewModel.AddToRequest(p);
            }
        }
    }
}
