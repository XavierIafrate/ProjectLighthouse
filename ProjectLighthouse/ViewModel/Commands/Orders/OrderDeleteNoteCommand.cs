using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class OrderDeleteNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private EditLMOWindow window;

        public OrderDeleteNoteCommand(EditLMOWindow w)
        {
            this.window = w;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not Note note) return;
            window.DeleteNote(note);
        }
    }
}
