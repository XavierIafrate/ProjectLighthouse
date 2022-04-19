using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public abstract class ScheduleItem : ISchedulableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FirebaseId { get; set; }
        public string Name { get; set; }
        public int TimeToComplete { get; set; }
        public DateTime StartDate { get; set; }
        public string AllocatedMachine { get; set; }

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
    }
}
