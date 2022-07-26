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
            App.ActiveViewModel = parameter.ToString();

            if (parameter.ToString() == "Schedule")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new ScheduleViewModel();
                viewModel.NavText = "Schedule";
            }
            else if (parameter.ToString() == "View Requests")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new RequestViewModel();
                viewModel.NavText = "Requests";
            }
            else if (parameter.ToString() == "New Request")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new NewRequestViewModel();
                viewModel.NavText = "New Request";
            }
            else if (parameter.ToString() == "Orders")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new OrderViewModel();
                viewModel.NavText = "Manufacture Orders";
            }
            else if (parameter.ToString() == "Bar Stock")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new BarStockViewModel();
                viewModel.NavText = "Bar Stock";
            }
            else if (parameter.ToString() == "Drawings")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DrawingBrowserViewModel();
                viewModel.NavText = "Technical Drawings";
            }
            else if (parameter.ToString() == "Calibration")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new CalibrationViewModel();
                viewModel.NavText = "Calibration";
            }
            else if (parameter.ToString() == "Quality Control")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new QualityCheckViewModel();
                viewModel.NavText = "Quality Control";
            }
            else if (parameter.ToString() == "Deliveries")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new DeliveriesViewModel();
                viewModel.NavText = "Deliveries";
            }
            else if (parameter.ToString() == "Dev Area / Debug")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DebugViewModel();
                viewModel.NavText = "Dev Area";
            }
            else if (parameter.ToString() == "Manage Users")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ManageUsersViewModel();
                viewModel.NavText = "Manage Users";
            }
            else if (parameter.ToString() == "Lathe Config")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new LatheViewModel();
                viewModel.NavText = "Lathe Configuration";
            }
            else if (parameter.ToString() == "Analytics")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new AnalyticsViewModel();
                viewModel.NavText = "Analytics";
            }
            else
            {
                App.ActiveViewModel = "";
            }

            viewModel.MainWindow.SelectButton(parameter.ToString());
        }
    }
}
