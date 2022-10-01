using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Quality;
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
            return App.CurrentUser.HasPermission(PermissionType.ModifyCalibration);
        }

        public void Execute(object parameter)
        {
            if (parameter is not int Id) return;
            viewModel.EditEquipment(Id);
        }
    }
}
