using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class GetLoginReportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ManageUsersViewModel viewModel;

        public GetLoginReportCommand(ManageUsersViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.GenerateReport();
        }
    }
}
