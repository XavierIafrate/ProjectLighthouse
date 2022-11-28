using ProjectLighthouse;
using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Research
{
    public class RemoveAttachmentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ResearchViewModel viewModel;

        public RemoveAttachmentCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(ProjectLighthouse.Model.Core.PermissionType.ModifyProjects);
        }

        public void Execute(object parameter)
        {
            viewModel.RemoveAttachment();
        }
    }
}
