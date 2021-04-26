using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class EditLMOItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;



        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
