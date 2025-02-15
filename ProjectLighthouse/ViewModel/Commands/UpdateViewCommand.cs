﻿using ProjectLighthouse.ViewModel.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Drawings;
using ProjectLighthouse.ViewModel.Orders;
using ProjectLighthouse.ViewModel.Programs;
using ProjectLighthouse.ViewModel.Quality;
using ProjectLighthouse.ViewModel.Requests;
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
            if (App.MainViewModel.SelectedViewModel != null)
            {
                if (!App.MainViewModel.SelectedViewModel.CanClose())
                {
                    viewModel.MainWindow.SelectButton(App.ActiveViewModel);
                    return;
                }
            }

            string targetView = parameter.ToString();

            App.ActiveViewModel = targetView;

            if (targetView == "Schedule")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ScheduleViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Harmonogram",
                    "Persian" => "برنامه",
                    "Welsh" => "Amserlen",
                    "Latvian" => "Grafiks",
                    _ => "Schedule"
                };
            }
            else if (targetView == "Requests")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new RequestViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Wyświetl Prośby",
                    "Persian" => "مشاهده درخواست ها",
                    "Welsh" => "Gweld Ceisiadau",
                    "Latvian" => "Pieprasījumi",
                    _ => "Requests"
                };
            }
            else if (targetView == "Orders")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new OrderViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Zamówienia Produkcje",
                    "Persian" => "سفارشات ساخت",
                    "Welsh" => "Gorchmynion Gweithgynhyrchu",
                    "Latvian" => "Ražošanas Pasūtījumi",
                    _ => "Manufacture Orders"
                };
            }
            else if (targetView == "Bar Stock")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new BarStockViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Materiał",
                    "Persian" => "ماده خام",
                    "Welsh" => "Deunydd Crai",
                    "Latvian" => "Izejviela",
                    _ => "Bar Stock"
                };
            }
            else if (targetView == "Drawings")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DrawingBrowserViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Rysunek Techniczney",
                    "Persian" => "نقشه های فنی",
                    "Welsh" => "Darluniau Technegol",
                    "Latvian" => "Tehniskie Rasējumi",
                    _ => "Technical Drawings"
                };
            }
            else if (targetView == "Programs")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ProgramManagerViewModel();
                viewModel.NavText = "Program Manager";
            }
            else if (targetView == "Calibration")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new CalibrationViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Kalibracja",
                    "Persian" => "تنظیم",
                    "Welsh" => "Calibradu",
                    "Latvian" => "Tehniskie Rasējumi",
                    _ => "Calibration"
                };
            }
            else if (targetView == "Deliveries")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DeliveriesViewModel();
                viewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Dostawy",
                    "Persian" => "تحویل",
                    "Welsh" => "Dosbarthu",
                    "Latvian" => "Piegādes",
                    _ => "Deliveries"
                };
            }
            else if (targetView == "Manage Users")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ManageUsersViewModel();
                viewModel.NavText = "Manage Users";
            }
            else if (targetView == "Lathe Config")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new MachineViewModel();
                viewModel.NavText = "Lathe Configuration";
            }
            else if (targetView == "Analytics")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new AnalyticsViewModel();
                viewModel.NavText = "Analytics";
            }
            else if (targetView == "Product Data")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new ProductManagerViewModel();
                viewModel.NavText = "Product Data";
            }
            else if (targetView == "DB Management")
            {
                viewModel.BetaWarningVis = Visibility.Collapsed;
                viewModel.SelectedViewModel = new DatabaseManagerViewModel();
                viewModel.NavText = "Database Manager";
            }
            else
            {
                App.ActiveViewModel = "";
            }

            viewModel.MainWindow.SelectButton(targetView);
        }
    }
}
