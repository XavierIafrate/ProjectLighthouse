using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class EditDeliveryNoteItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public DeliveriesViewModel viewModel;

        public EditDeliveryNoteItemCommand(DeliveriesViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.Role >= Model.UserRole.Scheduling;
        }

        public void Execute(object parameter)
        {
            if (parameter is int id)
            {
                viewModel.EditItem(id);
            }
        }
    }
}
