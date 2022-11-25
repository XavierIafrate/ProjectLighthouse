using LighthouseMonitoring.ViewModel;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace LighthouseMonitoring.Model
{
    public class MonitoringSystem : BaseViewModel
    {
        public List<Lathe> Lathes { get; set; } = new();
        public List<LatheState> LatheStates { get; set; } = new();

        #region Timer Variables
        private Timer? timer;

        private DateTime lastPolled;
        public DateTime LastPolled
        {
            get { return lastPolled; }
            set { lastPolled = value; OnPropertyChanged();  }
        }

        private double secondsSinceLastPoll;
        public double SecondsSinceLastPoll
        {
            get { return secondsSinceLastPoll; }
            set { secondsSinceLastPoll = value; OnPropertyChanged(); }
        }
        #endregion


        #region Setup
        public void Initialise()
        {
            SetupLathes();
            SetupTimer();

            MachineDataHelper.Initialise();

            timer?.Start();
        }

        private void SetupTimer()
        {
            timer = new(200);
            timer.Elapsed += Timer_Elapsed;
        }

        private void SetupLathes()
        {
            Lathes = DatabaseHelper.Read<Lathe>().Where(x => !x.OutOfService).ToList();
            for(int i = 0; i < Lathes.Count; i++)
            {
                LatheState newLatheState = new(Lathes[i]);

                newLatheState.OnNewError += NewLatheState_OnNewError;

                LatheStates.Add(new(Lathes[i]));
            }
        }
        #endregion

        private void NewLatheState_OnNewError(object? sender, List<string> e)
        {
            throw new NotImplementedException();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SecondsSinceLastPoll = (DateTime.Now - LastPolled).TotalSeconds;

            if(SecondsSinceLastPoll > Settings1.Default.PollInterval)
            {
                LastPolled = DateTime.Now;
                //Task.Run(() => PollMachines());
                PollMachines();
            }
        }

        private void PollMachines()
        {
            Debug.WriteLine($"{DateTime.Now:s} Polling Machines");

            for (int i = 0; i < LatheStates.Count; i++)
            {
                LatheState latheData = LatheStates[i];

                MachineData mtConnectInfo = MachineDataHelper.GetMachineData(latheData.Lathe);

                latheData.Update(mtConnectInfo);
            }
        }
    }
}
