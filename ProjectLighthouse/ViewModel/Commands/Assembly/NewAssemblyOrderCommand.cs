using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Assembly
{
    public class NewAssemblyOrderCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AssemblyOrdersViewModel viewModel;

        public NewAssemblyOrderCommand(AssemblyOrdersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.currentUser.CanApproveRequests;
        }

        public void Execute(object parameter)
        {
            viewModel.NewOrder();
        }
    }
}
