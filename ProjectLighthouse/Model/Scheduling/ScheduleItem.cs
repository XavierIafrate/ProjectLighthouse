using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.Model.Scheduling
{
    public abstract class ScheduleItem : BaseObject, ISchedulableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        private int timeToComplete;
        [UpdateWatch]
        public int TimeToComplete
        {
            get { return timeToComplete; }
            set { timeToComplete = value; OnPropertyChanged(); }
        }

        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; OnPropertyChanged(); }
        }

        public string AllocatedMachine { get; set; }
        [Ignore]
        public bool IsZeroSet { get; set; }


        [UpdateWatch]
        public string? ScheduleLock { get; set; }

        [Ignore]
        public ScheduleLock? ScheduleLockData
        {
            get 
            { 
                if(string.IsNullOrEmpty(ScheduleLock)) return null;
                try
                {
                    return ProductionSchedule.ScheduleLock.Parse(ScheduleLock);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    ScheduleLock = null;
                    return;
                }

                ScheduleLock = value.Serialise();
                OnPropertyChanged();
            }
        }

        public event EventHandler OnScheduleLockChanged;

        private List<Warning> warnings = new();
        [Ignore]
        public List<Warning> Warnings
        {
            get { return warnings; }
            set { warnings = value; }
        }

        private List<Advisory> advisories = new();
        [Ignore]
        public List<Advisory> Advisories
        {
            get { return advisories; }
            set { advisories = value; }
        }

        public event EventHandler OnAdvisoriesChanged;
        public event EventHandler OnWarningsChanged;

        internal void SetAdvisories(List<Advisory> orderAdvisories)
        {
            this.Advisories = orderAdvisories;
            OnAdvisoriesChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void SetWarnings(List<Warning> orderWarnings)
        {
            this.Warnings = orderWarnings;
            OnWarningsChanged?.Invoke(this, EventArgs.Empty);
        }


        public event Action EditMade;
        public void NotifyEditMade()
        {
            EditMade?.Invoke();
        }

        public event Action RequestToEdit;
        public void RequestEdit()
        {
            RequestToEdit?.Invoke();
        }

        public DateTime EndsAt()
        {
            return StartDate.AddSeconds(TimeToComplete);
        }

        public void UpdateStartDate(DateTime date, string machine)
        {
            StartDate = date == DateTime.MinValue ? DateTime.MinValue : date;
            AllocatedMachine = string.IsNullOrEmpty(machine) ? null : machine;

            string table;
            if (this is LatheManufactureOrder)
            {
                table = nameof(LatheManufactureOrder);
            }
            else if (this is MachineService)
            {
                table = nameof(MachineService);
            }
            else
            {
                throw new NotImplementedException();
            }

            string dbMachineEntry = string.IsNullOrEmpty(AllocatedMachine) ? "NULL" : $"'{AllocatedMachine}'";
            
            DatabaseHelper.ExecuteCommand($"UPDATE {table} SET StartDate = {StartDate.Ticks}, AllocatedMachine={dbMachineEntry} WHERE Id={Id}");
        }
    }
}
