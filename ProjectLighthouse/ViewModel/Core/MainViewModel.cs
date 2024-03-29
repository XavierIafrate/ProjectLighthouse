﻿using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Notifications;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Core
{
    public class MainViewModel : BaseViewModel
    {
        #region Vars

        private string navText = "Manufacture Orders";

        public string NavText
        {
            get { return navText; }
            set
            {
                navText = value;
                OnPropertyChanged();
            }
        }

        private Visibility betaWarningVis = Visibility.Collapsed;
        public Visibility BetaWarningVis
        {
            get { return betaWarningVis; }
            set
            {
                PoppyVis = remembrancePeriod && value != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
                betaWarningVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility poppyVis;

        public Visibility PoppyVis
        {
            get { return poppyVis; }
            set { poppyVis = value; OnPropertyChanged(); }
        }

        bool remembrancePeriod;


        private BaseViewModel _selectedViewModel;
        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                if (_selectedViewModel is IDisposable viewModel)
                {
                    viewModel.Dispose();
                }

                _selectedViewModel = value;
                NotificationsBarVis = Visibility.Collapsed;

                OnPropertyChanged();
            }
        }

        private int notCount;
        public int NotCount
        {
            get { return notCount; }
            set
            {
                notCount = value;
                OnPropertyChanged();
            }
        }

        private bool noNewNotifications;
        public bool NoNewNotifications
        {
            get { return noNewNotifications; }
            set
            {
                noNewNotifications = value;
                OnPropertyChanged();
            }
        }

        private bool noNotifications = true;
        public bool NoNotifications
        {
            get { return noNotifications; }
            set
            {
                noNotifications = value;
                OnPropertyChanged();
            }
        }


        private List<Notification> nots;
        public List<Notification> Notifications
        {
            get { return nots; }
            set
            {
                nots = value;
                OnPropertyChanged();
            }
        }

        private Visibility notificationsBarVis = Visibility.Collapsed;
        public Visibility NotificationsBarVis
        {
            get { return notificationsBarVis; }
            set
            {
                notificationsBarVis = value;
                OnPropertyChanged();
            }
        }

        public UpdateViewCommand UpdateViewCommand { get; set; }
        public EditSettingsCommand EditCommand { get; set; }
        public ToggleNotificationsBarViewCommand ToggleShowNotsCommand { get; set; }
        public ReadAllNotificationsCommand ReadAllCommand { get; set; }

        public MainWindow MainWindow { get; set; }

        #endregion Vars

        public static void EditSettings()
        {
            EditSettingsWindow window = new();
            window.ShowDialog();
        }

        public MainViewModel()
        {
            NoNewNotifications = true;
            EditCommand = new();
            UpdateViewCommand = new(this);
            ToggleShowNotsCommand = new(this);
            ReadAllCommand = new();

            remembrancePeriod = DateTime.Today.Day < 14 && DateTime.Today.Month == 11;
        }


        public bool LoginRoutine()
        {
            LoginWindow login = new();
            login.ShowDialog();

            if (login.auth_user == null)
            {
                return false;
            }

            App.CurrentUser = login.auth_user;
            App.CurrentUser.UserPermissions = DatabaseHelper.Read<Permission>().Where(x => x.UserId == App.CurrentUser.Id).ToList();


            string TargetView = string.IsNullOrEmpty(App.CurrentUser.DefaultView)
                ? "Orders"
                : App.CurrentUser.DefaultView;

            if (MainWindow != null)
            {
                UpdateViewCommand.Execute(TargetView);
                if (App.CurrentUser.DefaultMenuWidth != null)
                {
                    MainWindow.MainGrid.ColumnDefinitions[0].Width = new((double)App.CurrentUser.DefaultMenuWidth);
                }
            }

            return true;
        }

        public void ToggleNotificationsBarVisibility()
        {
            NotificationsBarVis = NotificationsBarVis == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}