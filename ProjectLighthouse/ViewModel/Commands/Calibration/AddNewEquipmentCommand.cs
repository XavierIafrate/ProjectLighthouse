using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Quality;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Calibration
{
    public class AddNewEquipmentCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        CalibrationViewModel viewModel;

        public AddNewEquipmentCommand(CalibrationViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.ModifyCalibration);
        }

        public void Execute(object parameter)
        {
            viewModel.AddNewEquipment();
        }
    }
}
