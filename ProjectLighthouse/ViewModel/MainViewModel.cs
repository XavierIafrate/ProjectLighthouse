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

        private BaseViewModel _selectedViewModel = new OrderViewModel();
        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                Debug.WriteLine(String.Format("SelectedViewModel updated to {0}", _selectedViewModel.ToString()));
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public ICommand UpdateViewCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public void EditSettings()
        {
            EditSettingsWindow window = new EditSettingsWindow();
            window.ShowDialog();
        }

        public MainViewModel()
        {
            EditCommand = new EditSettingsCommand(this);
            UpdateViewCommand = new UpdateViewCommand(this);
            if (App.currentUser == null)
            {
                LoginWindow login = new LoginWindow();
                login.ShowDialog();

                if (login.auth_user == null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    App.currentUser = login.auth_user;
                }
            }

        }
    }
}
