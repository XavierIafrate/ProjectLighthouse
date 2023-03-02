using ProjectLighthouse.ViewModel.Drawings;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Drawings
{
    public class ConvertToDevelopmentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private DrawingBrowserViewModel viewModel;
        public ConvertToDevelopmentCommand(DrawingBrowserViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role == Model.Administration.UserRole.Administrator;
        }

        public void Execute(object parameter)
        {
            viewModel.ConvertToDevelopment();
        }
    }
}
