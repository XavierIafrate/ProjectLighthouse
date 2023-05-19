using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class OrderSaveEditNoteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private EditLMOWindow window;
        public OrderSaveEditNoteCommand(EditLMOWindow w)
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
            window.SaveNoteEdit(note);
        }
    }
}
