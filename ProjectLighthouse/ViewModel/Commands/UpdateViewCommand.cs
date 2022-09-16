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
                viewModel.BetaWarningVis = Visibility.Hidden;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new ScheduleViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Harmonogram",
                    "Persian" => "برنامه",
                    "Welsh" => "Amserlen",
                    _ => "Schedule"
                };
            }
            else if (parameter.ToString() == "View Requests")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new RequestViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Wyświetl Prośby",
                    "Persian" => "مشاهده درخواست ها",
                    "Welsh" => "Gweld Ceisiadau",
                    _ => "View Requests"
                };
            }
            else if (parameter.ToString() == "New Request")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new NewRequestViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Nowe Żądanie",
                    "Persian" => "درخواست جدید",
                    "Welsh" => "Cais Newydd",
                    _ => "New Request"
                };
            }
            else if (parameter.ToString() == "Orders")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new OrderViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Zamówienia Produkcje",
                    "Persian" => "سفارشات ساخت",
                    "Welsh" => "Gorchmynion Gweithgynhyrchu",
                    _ => "Manufacture Orders"
                };
            }
            else if (parameter.ToString() == "Bar Stock")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new BarStockViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Materiał",
                    "Persian" => "ماده خام",
                    "Welsh" => "Deunydd Crai",
                    _ => "Bar Stock"
                };
            }
            else if (parameter.ToString() == "Drawings")
            {
                viewModel.BetaWarningVis = Visibility.Visible;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DrawingBrowserViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Rysunek Techniczney",
                    "Persian" => "نقشه های فنی",
                    "Welsh" => "Darluniau Technegol",
                    _ => "Technical Drawings"
                };
            }
            else if (parameter.ToString() == "Calibration")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new CalibrationViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Kalibracja",
                    "Persian" => "تنظیم",
                    "Welsh" => "Calibradu",
                    _ => "Calibration"
                };
            }
            else if (parameter.ToString() == "Quality Check")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new QualityCheckViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Kontrola Jakości",
                    "Persian" => "بررسی کیفیت",
                    "Welsh" => "Gwiriad Ansawdd",
                    _ => "Quality Check"
                };
            }
            else if (parameter.ToString() == "Deliveries")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.MiBVis = Visibility.Visible;
                viewModel.SelectedViewModel = new DeliveriesViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Dostawy",
                    "Persian" => "تحویل",
                    "Welsh" => "Dosbarthu",
                    _ => "Deliveries"
                };
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
