using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class MachineStatsViewModel : BaseViewModel
    {
        public GetStatsCommand getStatsCommand { get; set; }
        public ObservableCollection<MachineStatistics> StatsList { get; set; }
        public string estimation { get; set; }
        private DispatcherTimer dispatcherTimer { get; set; }
        public Visibility NoConnectionVis { get; set; }
        public Visibility StatsVis { get; set; }

        public MachineStatsViewModel()
        {
            NoConnectionVis = Visibility.Hidden;
            StatsVis = Visibility.Visible;
            getStatsCommand = new GetStatsCommand(this);
            estimation = "";
            StatsList = new ObservableCollection<MachineStatistics>();
            getStats();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            getStats();
        }

        public async void getStats()
        {
            List<MachineStatistics> statsList = await MachineStatsHelper.GetStats();
            if(statsList == null)
            {
                dispatcherTimer.Stop();
                NoConnectionVis = Visibility.Visible;
                StatsVis = Visibility.Collapsed;
                OnPropertyChanged("NoConnectionVis");
                OnPropertyChanged("StatsVis");
                return;
            }
            StatsList.Clear();
            foreach (var item in statsList)
            {
                StatsList.Add(item);
            }
            OnPropertyChanged("StatsList");
        }

    }
}
