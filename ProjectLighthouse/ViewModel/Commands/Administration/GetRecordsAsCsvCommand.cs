using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ViewModel.Commands.Administration
{
    public class GetRecordsAsCsvCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private DatabaseManagerViewModel viewModel;

        public GetRecordsAsCsvCommand(DatabaseManagerViewModel vm)
        {
            viewModel = vm;
        }



        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not string p)
            {
                return;
            }

            viewModel.GetCsv(p);
        }
    }
}
