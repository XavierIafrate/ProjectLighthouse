using System;
using System.Windows.Input;
using ProjectLighthouse.ViewModel.Core;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class EditSettingsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MainViewModel.EditSettings();
        }
    }
}
