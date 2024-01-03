using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Programs;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Programs
{
    public class OpenCommitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ProgramManagerViewModel viewModel;
        public OpenCommitCommand(ProgramManagerViewModel vm)
        {
            this.viewModel = vm;
        }


        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not NcProgramCommit commit)
            {
                NotificationManager.NotifyHandledException(new Exception("OpenCommitCommand: Parameter passed to Execute is not a commit"));
                return;
            }

            viewModel.OpenCommit(commit);
        }
    }
}
