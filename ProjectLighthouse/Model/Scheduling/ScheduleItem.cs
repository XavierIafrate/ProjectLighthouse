using SQLite;
using System;

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

        public DateTime StartDate { get; set; }
        public string AllocatedMachine { get; set; }
        [Ignore]
        public bool IsZeroSet { get; set; }

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
    }
}
