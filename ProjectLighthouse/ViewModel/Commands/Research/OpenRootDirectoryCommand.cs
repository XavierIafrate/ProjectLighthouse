using ProjectLighthouse.Model.Core;
using System;
using System.Windows.Input;
using ViewModel.Research;

namespace ProjectLighthouse.ViewModel.Commands.Research
{
    public class OpenRootDirectoryCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ResearchViewModel viewModel;

        public OpenRootDirectoryCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.CreateProject);
        }

        public void Execute(object parameter)
        {
            viewModel.OpenRoot();
        }
    }
}
