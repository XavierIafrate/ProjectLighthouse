using System;
using System.Windows.Input;
using ViewModel.Research;

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
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.ModifyProject);
        }

        public void Execute(object parameter)
        {
            viewModel.AddArchetype();
        }
    }
}
