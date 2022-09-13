using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class WithdrawDrawingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DrawingBrowserViewModel viewModel;

        public WithdrawDrawingCommand(DrawingBrowserViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role == Model.UserRole.Administrator;
        }

        public void Execute(object parameter)
        {
            viewModel.WithdrawDrawing();
        }
    }
}
