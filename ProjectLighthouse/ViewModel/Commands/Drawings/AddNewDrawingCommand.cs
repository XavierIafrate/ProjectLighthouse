using ProjectLighthouse.ViewModel.Drawings;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
{
    public class AddNewDrawingCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private DrawingBrowserViewModel viewModel;

        public AddNewDrawingCommand(DrawingBrowserViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddNewDrawing();
        }
    }
}
