using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Research
{
    public class AddProjectCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ResearchViewModel viewModel;

        public AddProjectCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageProjects);
        }

        public void Execute(object parameter)
        {
            viewModel.CreateProject();
        }
    }
}
