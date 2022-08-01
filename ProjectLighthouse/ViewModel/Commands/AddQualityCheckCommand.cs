using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class AddQualityCheckCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private QualityCheckViewModel viewModel;

        public AddQualityCheckCommand(QualityCheckViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddQualityCheck();
        }
    }
}
