using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddProductGroupCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ProductManagerViewModel viewModel;

        public AddProductGroupCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddProductGroup();
        }
    }
}
