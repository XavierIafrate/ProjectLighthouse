using LighthouseMonitoring.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LighthouseMonitoring.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        public MainWindow MainWindow;

		private BaseViewModel displayViewModel;
		public BaseViewModel DisplayViewModel
		{
			get { return displayViewModel; }
			set 
			{ 
				displayViewModel = value; 
				OnPropertyChanged(); 
			}
		}

		private Timer timer;

		public MainViewModel(MainWindow window)
		{

			MainWindow = window;
			MainWindow.DataContext = this;

			DisplayText = "Controller";
			
			MainWindow.Show();

			//timer = new(2500);
			//timer.Elapsed += Timer_Elapsed;
			//timer.Start();
			DisplayViewModel = new MonitoringViewModel();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (DisplayViewModel is MonitoringViewModel)
			{
				DisplayViewModel = new SettingsViewModel();
			}
			else
			{
				DisplayViewModel = new MonitoringViewModel();
			}
		}
	}
}
