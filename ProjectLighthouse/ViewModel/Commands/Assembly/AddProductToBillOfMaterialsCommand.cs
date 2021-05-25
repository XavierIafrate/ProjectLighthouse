using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Assembly
{
    public class AddProductToBillOfMaterialsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AssemblyProductsViewModel viewModel;

        public AddProductToBillOfMaterialsCommand(AssemblyProductsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //viewModel.AddProductToBOM();
        }
    }
}
