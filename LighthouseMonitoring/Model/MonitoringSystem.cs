using LighthouseMonitoring.ViewModel;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace LighthouseMonitoring.Model
{
    public class MonitoringSystem : BaseViewModel
    {
        public List<Lathe> Lathes { get; set; } = new();
        public List<LatheState> LatheStates { get; set; } = new();

        private Timer timer;

        private DateTime lastPolled;

        public DateTime LastPolled
        {
            get { return lastPolled; }
            set { lastPolled = value; OnPropertyChanged();  }
        }

        public void Initialise()
        {
            Lathes = DatabaseHelper.Read<Lathe>().Where(x => !x.OutOfService).ToList();

            MachineDataHelper.Initialise();

            timer = new(200);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();


            MachineDataHelper.GetMachineData(Lathes.First());
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LastPolled = DateTime.Now;
            Debug.WriteLine($"Last Polled set to : {LastPolled:s}");
        }


    }
}
