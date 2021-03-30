using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class MachineStatsViewModel : BaseViewModel
    {
        public GetStatsCommand getStatsCommand { get; set; }
        public MachineStatistics CitizenOne { get; set; }
        public string estimation { get; set; }
        

        public MachineStatsViewModel()
        {
            getStatsCommand = new GetStatsCommand(this);
            estimation = "";
            getStats();

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            getStats();
        }

        public void getStats()
        {
            CitizenOne = MachineStatsHelper.GetStats();
            CitizenOne.MachineID = "Citizen One";

            string MessageBody = String.Format(
                "Citizen 1 is in {1} mode. Parts counter is on {2} of {3}. Estimated completion in {4}.", 
                App.currentUser.FirstName,
                CitizenOne.ControllerMode.ToLower(),
                CitizenOne.PartCountAll,
                CitizenOne.PartCountTarget,
                CitizenOne.EstimateCompletion());

            estimation = CitizenOne.EstimateCompletion();

            //SMSHelper.SendText("+447979606705", MessageBody);
            OnPropertyChanged("CitizenOne");
            OnPropertyChanged("estimation");
        }
    }
}
