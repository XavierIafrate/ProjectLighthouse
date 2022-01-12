using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Calibration
{
    public class EditEquipmentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private CalibrationViewModel viewModel;

        public EditEquipmentCommand(CalibrationViewModel vm)
        {
            viewModel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.EditEquipment();
        }
    }
}
