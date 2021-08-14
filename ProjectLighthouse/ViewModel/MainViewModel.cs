using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using System;
using System.Diagnostics;
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
                OnPropertyChanged("NavText");
            }
        }

        private Visibility betaWarningVis = Visibility.Collapsed;
        public Visibility BetaWarningVis
        {
            get { return betaWarningVis; }
            set
            {
                betaWarningVis = value;
                OnPropertyChanged("BetaWarningVis");
            }
        }

        private BaseViewModel _selectedViewModel;
        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                Debug.WriteLine(String.Format("SelectedViewModel updated to {0}", _selectedViewModel.ToString()));
                OnPropertyChanged("SelectedViewModel");
                //OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public ICommand UpdateViewCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public MainWindow MainWindow { get; set; }

        #endregion

        public void EditSettings()
        {
            EditSettingsWindow window = new();
            window.ShowDialog();
        }

        public MainViewModel()
        {
            Debug.WriteLine("Init: MainViewModel");

            EditCommand = new EditSettingsCommand(this);
            UpdateViewCommand = new UpdateViewCommand(this);
        }

        public void LoginRoutine()
        {
            if (App.CurrentUser == null)
            {
                LoginWindow login = new();

                login.ShowDialog();

                if (login.auth_user == null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    App.CurrentUser = login.auth_user;
                    string TargetView = string.IsNullOrEmpty(App.CurrentUser.DefaultView)
                        ? "Orders"
                        : App.CurrentUser.DefaultView;
                    if (MainWindow != null)
                        UpdateViewCommand.Execute(TargetView);
                }
            }
        }
    }
}
