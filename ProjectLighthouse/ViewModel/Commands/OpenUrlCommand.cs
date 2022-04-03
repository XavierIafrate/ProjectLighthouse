using ProjectLighthouse.Model;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class OpenUrlCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public CheckSheetsViewModel viewModel;
        public OpenUrlCommand(CheckSheetsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            Product p = (Product)parameter;
            if (p == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(p.WebUrl);
        }

        public void Execute(object parameter)
        {
            Product p = (Product)parameter;
            viewModel.OpenWebsite(p.WebUrl);
        }
    }
}
