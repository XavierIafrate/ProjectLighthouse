using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class RejectDrawingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public DrawingBrowserViewModel viewModel;

        public RejectDrawingCommand(DrawingBrowserViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanApproveDrawings;
        }

        public void Execute(object parameter)
        {
            viewModel.RejectDrawing();
        }
    }
}
