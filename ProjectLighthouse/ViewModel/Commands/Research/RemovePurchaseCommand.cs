using ProjectLighthouse;
using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Research
{
    public class RemovePurchaseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ResearchViewModel viewModel;

        public RemovePurchaseCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(ProjectLighthouse.Model.Core.PermissionType.ModifyProjects);
        }

        public void Execute(object parameter)
        {
            viewModel.RemovePurchase();
        }
    }
}
