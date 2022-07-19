using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
