using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class NewSpecialPartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private NewRequestViewModel viewModel;

        public NewSpecialPartCommand(NewRequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddSpecialRequest();
        }
    }
}
