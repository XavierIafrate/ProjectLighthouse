using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Orders
{
    public class OrderViewModelRelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private NewOrderViewModel viewModel;
        public OrderViewModelRelayCommand(NewOrderViewModel vm)
        {
            this.viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter is not string argument) return false;

            if (argument == "edit")
            {
                return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateOrder);
            }

            if (argument == "save")
            {
                return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateOrder);

            }

            if (argument == "cancel")
            {
                return App.CurrentUser.HasPermission(Model.Core.PermissionType.UpdateOrder);
            }

            throw new ArgumentException("Unrecognised parameter passed to command");
        }

        public void Execute(object parameter)
        {
            if (parameter is not string argument)
            {
                throw new ArgumentException("Unrecognised parameter passed to command");
            };

            if (argument == "edit")
            {
                this.viewModel.EnterEditMode();
                return;
            }

            if (argument == "save")
            {
                this.viewModel.ExitEditMode(save: true);
                return;
            }

            if (argument == "cancel")
            {
                this.viewModel.ExitEditMode(save:false);
                return;
            }

            throw new ArgumentException("Unrecognised parameter passed to command");

        }
    }
}
