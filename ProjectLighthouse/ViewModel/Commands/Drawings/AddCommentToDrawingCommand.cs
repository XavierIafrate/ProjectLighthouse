using ProjectLighthouse.ViewModel.Drawings;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
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
