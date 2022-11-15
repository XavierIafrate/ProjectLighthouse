using SQLite;
using System;

namespace ProjectLighthouse.Model.Scheduling
{
    public abstract class ScheduleItem : ISchedulableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        [UpdateWatch]
        public int TimeToComplete { get; set; }
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


        public class UpdateWatch : Attribute
        {

        }
    }
}
