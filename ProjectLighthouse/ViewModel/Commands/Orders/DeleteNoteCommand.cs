using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Core;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class DeleteNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private BaseViewModel viewModel;

        public DeleteNoteCommand(BaseViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not Note note)
            {
                throw new ArgumentException("Command expects a note as parameter");
            }

            viewModel.DeleteNote(note);
        }
    }
}
