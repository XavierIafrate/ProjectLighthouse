using System;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private MainViewModel viewModel;
        public event EventHandler CanExecuteChanged;

        public UpdateViewCommand(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter.ToString() == "Schedule")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ScheduleViewModel();
                viewModel.NavText = "Schedule";
            }
            else if (parameter.ToString() == "View Requests")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new RequestViewModel();
                viewModel.NavText = "Requests";
            }
            else if (parameter.ToString() == "New Request")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new NewRequestViewModel();
                viewModel.NavText = "New Request";
            }
            else if (parameter.ToString() == "Orders")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new OrderViewModel();
                viewModel.NavText = "Manufacture Orders";
            }
            else if (parameter.ToString() == "Runtime")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new MachineStatsViewModel();
                viewModel.NavText = "Machine Statistics";
            }
            else if (parameter.ToString() == "Deliveries")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new DeliveriesViewModel();
                viewModel.NavText = "Deliveries";
            }
            else if (parameter.ToString() == "Manage Products")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new AssemblyProductsViewModel();
                viewModel.NavText = "Assembly";
            }
            else if (parameter.ToString() == "Bill of Materials")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new BillOfMaterialsViewModel();
                viewModel.NavText = "Bill of Materials";
            }
            else if (parameter.ToString() == "Assembly Orders")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new AssemblyOrdersViewModel();
                viewModel.NavText = "Assembly Orders";
            }
            else if (parameter.ToString() == "Manage Users")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new ManageUsersViewModel();
                viewModel.NavText = "Manage Users";
            }
            else if (parameter.ToString() == "Analytics")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.SelectedViewModel = new AnalyticsViewModel();
                viewModel.NavText = "Analytics";
            }
            viewModel.window.SelectButton(parameter.ToString());
        }
    }
}
