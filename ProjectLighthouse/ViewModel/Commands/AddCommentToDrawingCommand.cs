using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class AddCommentToDrawingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DrawingBrowserViewModel viewModel;

        public AddCommentToDrawingCommand(DrawingBrowserViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddCommentToDrawing();
        }
    }
}
