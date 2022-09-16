using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class IssueBarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private BarStockViewModel viewModel;
        
        public IssueBarCommand(BarStockViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.CanIssueBarStock;
        }

        public void Execute(object parameter)
        {
            viewModel.CreateBarIssue();
        }
    }
}
