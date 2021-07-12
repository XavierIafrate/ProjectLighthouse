using ProjectLighthouse.Model;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Assembly
{
    public class EditAssemblyOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private AssemblyOrdersViewModel viewModel;

        public EditAssemblyOrderCommand(AssemblyOrdersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanEditLMOs;
        }

        public void Execute(object parameter)
        {
            viewModel.EditOrder(parameter as AssemblyManufactureOrder);
        }
    }
}
