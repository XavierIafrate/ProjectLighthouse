using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Research
{
    public class NewProjectArchetypeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ResearchViewModel viewModel;

        public NewProjectArchetypeCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageProjects);
        }

        public void Execute(object parameter)
        {
            viewModel.AddArchetype();
        }
    }
}
