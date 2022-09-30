using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
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
                betaWarningVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility miBVis = Visibility.Collapsed;
        public Visibility MiBVis
        {
            get { return miBVis; }
            set
            {
                miBVis = value;
                OnPropertyChanged();
            }
        }

        public Visibility DataRefreshBadgeVis { get; set; }

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
                DataRefreshBadgeVis = (_selectedViewModel is IRefreshableViewModel) && App.CurrentUser.EnableDataSync
                    ? Visibility.Collapsed
                    : Visibility.Collapsed;
                OnPropertyChanged(nameof(DataRefreshBadgeVis));

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

        public ICommand UpdateViewCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ToggleNotificationsBarViewCommand ToggleShowNotsCommand { get; set; }
        public ReadAllNotificationsCommand ReadAllCommand { get; set; }

        public MainWindow MainWindow { get; set; }

        #endregion Vars

        public void EditSettings()
        {
            EditSettingsWindow window = new();
            window.ShowDialog();
        }

        public MainViewModel()
        {
            EditCommand = new EditSettingsCommand(this);
            UpdateViewCommand = new UpdateViewCommand(this);
            ToggleShowNotsCommand = new(this);
            ReadAllCommand = new();
            NoNotifications = true;
            OnPropertyChanged(nameof(NoNotifications));
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

            string TargetView = string.IsNullOrEmpty(App.CurrentUser.DefaultView)
                ? "Orders"
                : App.CurrentUser.DefaultView;

            if (MainWindow != null)
            {
                UpdateViewCommand.Execute(TargetView);
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