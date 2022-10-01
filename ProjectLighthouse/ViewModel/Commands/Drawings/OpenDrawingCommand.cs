using System;
using ProjectLighthouse.ViewModel.Drawings;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
{
    public class OpenDrawingCommand : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;
        private DrawingBrowserViewModel viewModel;

        public OpenDrawingCommand(DrawingBrowserViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.OpenPdfDrawing();
        }
    }
}
