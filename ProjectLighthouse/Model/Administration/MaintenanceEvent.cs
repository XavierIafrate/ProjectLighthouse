﻿using ProjectLighthouse.Model.Core;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Administration
{
    public class MaintenanceEvent : IAutoIncrementPrimaryKey
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
                // TODO : Figure this out
                return Active ? StartingDate.AddMonths(IntervalMonths) : LastCompleted;
            }
        }

        //public DateTime GetNextDue()
        //{

        //}

        public override string ToString()
        {
            return Description;
        }
    }
}