using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.View.AssemblyViews;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdateAssemblyOrderItemCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private EditAssemblyOrderWindow EditWindow;

        public UpdateAssemblyOrderItemCommand(EditAssemblyOrderWindow w)
        {
            EditWindow = w;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not AssemblyOrderItem item)
                return;
            EditWindow.UpdateItem(item);
        }
    }
}
