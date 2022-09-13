using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Calibration
{
    public class AddCertificateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        CalibrationViewModel viewModel;

        public AddCertificateCommand(CalibrationViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanAddCalibrationCertificates;
        }

        public void Execute(object parameter)
        {
            viewModel.AddCertificate();
        }
    }
}
