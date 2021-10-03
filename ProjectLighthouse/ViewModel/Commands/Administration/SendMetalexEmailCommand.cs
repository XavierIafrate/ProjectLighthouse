using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class SendMetalexEmailCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public BarStockViewModel ViewModel { get; set; }

        public SendMetalexEmailCommand(BarStockViewModel vm)
        {
            ViewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.UserRole is "admin" or "Purchasing"; 
        }

        public void Execute(object parameter)
        {
            ViewModel.ComposeEmail();
        }
    }
}
