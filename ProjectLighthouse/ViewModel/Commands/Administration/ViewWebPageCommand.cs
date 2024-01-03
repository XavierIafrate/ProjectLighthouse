using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class ViewWebPageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ProductManagerViewModel viewModel;

        public ViewWebPageCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ViewWebPage();
        }
    }
}
