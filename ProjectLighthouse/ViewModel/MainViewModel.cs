using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainViewModel()
        {
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
