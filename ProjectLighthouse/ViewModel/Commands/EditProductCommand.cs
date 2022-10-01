using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Requests;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class EditProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RequestViewModel viewModel;

        public EditProductCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.EditProduct();
        }
    }
}
