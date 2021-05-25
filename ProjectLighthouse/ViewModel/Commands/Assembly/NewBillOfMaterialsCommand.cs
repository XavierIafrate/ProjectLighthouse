using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Assembly
{
    public class NewBillOfMaterialsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AssemblyProductsViewModel viewModel;

        public NewBillOfMaterialsCommand(AssemblyProductsViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.currentUser.CanCreateAssemblyProducts;
        }

        public void Execute(object parameter)
        {
            //viewModel.CreateNewBillOfMaterials();
        }
    }
}
