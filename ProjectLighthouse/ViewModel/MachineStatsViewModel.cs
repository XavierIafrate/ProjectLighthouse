using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<MachineStatistics> StatsList { get; set; }
        public string estimation { get; set; }
        

        public MachineStatsViewModel()
        {
            getStatsCommand = new GetStatsCommand(this);
            estimation = "";
            StatsList = new ObservableCollection<MachineStatistics>();
            getStats();

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            getStats();
        }

        public void getStats()
        {
            List<MachineStatistics> statsList = MachineStatsHelper.GetStats();

            StatsList.Clear();
            foreach(var item in statsList)
            {
                StatsList.Add(item);
            }
            //if(StatsList.FirstOrDefault().PartCountRemaining == 10)
            //{
            //    SendText();
            //}
            OnPropertyChanged("StatsList");
        }

        public void SendText()
        {

            MachineStatistics target = StatsList.Where(n => n.MachineID == "Citizen One").FirstOrDefault();

            string MessageBody = String.Format(
                "Citizen 1 is in {1} mode. Parts counter is on {2} of {3}. Estimated completion in {4}.",
                App.currentUser.FirstName,
                target.ControllerMode.ToLower(),
                target.PartCountAll,
                target.PartCountTarget,
                target.EstimateCompletion());
            SMSHelper.SendText("+447979606705", MessageBody);
        }
    }
}
