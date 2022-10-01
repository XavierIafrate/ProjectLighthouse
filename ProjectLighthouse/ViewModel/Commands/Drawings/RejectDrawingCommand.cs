using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Drawings;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
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
            return App.CurrentUser.HasPermission(PermissionType.ApproveDrawings);
        }

        public void Execute(object parameter)
        {
            viewModel.RejectDrawing();
        }
    }
}
