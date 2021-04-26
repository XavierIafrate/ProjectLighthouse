using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdateViewCommand : ICommand
    {

        private MainViewModel viewModel;

        public UpdateViewCommand(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            Debug.WriteLine(String.Format("UpdateViewParameter: {0}", parameter.ToString()));

            if (parameter.ToString() == "Schedule")
            {
                viewModel.SelectedViewModel = new ScheduleViewModel();
                viewModel.NavText = "Schedule";
            }
            else if (parameter.ToString() == "Requests")
            {
                viewModel.SelectedViewModel = new RequestViewModel();
                viewModel.NavText = "Requests";
            }
            else if (parameter.ToString() == "NewRequest")
            {
                viewModel.SelectedViewModel = new NewRequestViewModel();
                viewModel.NavText = "New Request";
            }
            else if (parameter.ToString() == "Orders")
            {
                viewModel.SelectedViewModel = new OrderViewModel();
                viewModel.NavText = "Manufacture Orders";
            }
            else if (parameter.ToString() == "Machine Stats")
            {
                viewModel.SelectedViewModel = new MachineStatsViewModel();
                viewModel.NavText = "Machine Statistics";
            }
            else if (parameter.ToString() == "Products")
            {
                MessageBox.Show("Not implemented yet!");
                //viewModel.SelectedViewModel = new OrderViewModel();
                //viewModel.NavText = "Manufacture Orders";
            }
            else if (parameter.ToString() == "Performance")
            {
                MessageBox.Show("Not implemented yet!");
                //viewModel.SelectedViewModel = new OrderViewModel();
                //viewModel.NavText = "Manufacture Orders";
            }
            else if (parameter.ToString() == "BarStock")
            {
                MessageBox.Show("Not implemented yet!");
                //viewModel.SelectedViewModel = new OrderViewModel();
                //viewModel.NavText = "Manufacture Orders";
            }
        }
    }
}
