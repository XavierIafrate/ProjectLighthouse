using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Assembly
{
    public class NewRoutingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        AssemblyProductsViewModel viewModel;

        public NewRoutingCommand(AssemblyProductsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanCreateAssemblyProducts;
        }

        public void Execute(object parameter)
        {
            viewModel.CreateNewRouting();
        }
    }
}
