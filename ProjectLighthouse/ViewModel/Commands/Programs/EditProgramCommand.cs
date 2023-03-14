using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Programs;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Programs
{
    public class EditProgramCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ProgramManagerViewModel viewModel;

        public EditProgramCommand(ProgramManagerViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.ConfigurePrograms);
        }

        public void Execute(object parameter)
        {
            if (parameter is not null)
            {
                viewModel.EditProgram();
            }
            else
            {
                viewModel.AddProgram();
            }
        }
    }
}
