using System;
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
            return App.CurrentUser.CanCreateSpecial;
        }

        public void Execute(object parameter)
        {
            viewModel.AddSpecialRequest();
        }
    }
}
