using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddCertificate();
        }
    }
}
