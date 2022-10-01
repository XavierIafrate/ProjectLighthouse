using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Quality;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Calibration
{
    public class RecordVisualCheckCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        CalibrationViewModel viewModel;

        public RecordVisualCheckCommand(CalibrationViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.UpdateCalibration);
        }

        public void Execute(object parameter)
        {
            viewModel.RecordVisualCheck();
        }
    }
}
