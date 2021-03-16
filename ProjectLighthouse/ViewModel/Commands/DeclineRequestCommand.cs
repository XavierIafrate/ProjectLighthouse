using ProjectLighthouse.View.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class DeclineRequestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RequestViewModel viewModel { get; set; }

        public DeclineRequestCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.DeclineRequest();
        }
    }
}
