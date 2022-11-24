using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ViewModel.Research;

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
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.CreateProject);
        }

        public void Execute(object parameter)
        {
            viewModel.CreateProject();
        }
    }
}
