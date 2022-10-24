using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class EditLatheCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private LatheViewModel viewModel;
        public EditLatheCommand(LatheViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.EditLathe();
        }
    }
}
