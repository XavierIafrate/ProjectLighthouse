using ProjectLighthouse.ViewModel.Drawings;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
{
    public class EditSpecificationCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private DrawingBrowserViewModel viewModel;

        public EditSpecificationCommand(DrawingBrowserViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ApproveDrawings);
        }

        public void Execute(object parameter)
        {
            viewModel.EditSpecification();
        }
    }
}
