using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Scheduling;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Administration
{
    public class MaintenanceEvent : ScheduleItem, IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Lathe { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotNull]
        public DateTime StartingDate { get; set; }
        public DateTime LastCompleted { get; set; }
        [NotNull]
        public int IntervalMonths { get; set; }
        public bool Active { get; set; }
        public bool RequireDocumentation { get; set; }

        [Ignore]
        public bool IsDue { 
            get {
                return Active && 
                      (LastCompleted == DateTime.MinValue 
                    || LastCompleted < DateTime.Now.AddMonths(-1 * IntervalMonths));
            } 
        }

        [Ignore]
        public DateTime NextDue
        {
            get
            {
                return Active ? StartingDate.AddMonths(IntervalMonths) : LastCompleted;
            }
        }

        public DateTime GetNextDue()
        {
            if (StartingDate > DateTime.Now)
            {
                return StartingDate;
            }

            if (LastCompleted == DateTime.MinValue)
            {
                return StartingDate;
            }

            int paddingWeeks = 3;
            if (IntervalMonths == 1)
            {
                paddingWeeks = 1;
            }

            DateTime dateTime = StartingDate;
            while (dateTime < LastCompleted.AddDays(7*paddingWeeks))
            {
                dateTime.AddMonths(1);
            }

            return dateTime;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
