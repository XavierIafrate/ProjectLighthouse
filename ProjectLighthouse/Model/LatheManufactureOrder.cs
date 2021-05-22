using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class LatheManufactureOrder : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string POReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public int TimeToComplete { get; set; } // in seconds

        public bool IsComplete { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsUrgent { get; set; }
        public int SettingTime { get; set; }

        public string AllocatedMachine { get; set; }
        public string AllocatedSetter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SettingFinished { get; set; }
        public DateTime CompletedAt { get; set; }

        public bool IsReady { get; set; }
        public bool HasProgram { get; set; }
        public bool HasStarted { get; set; }
        public string BarID { get; set; }
        public double NumberOfBars { get; set; }

        public object Clone()
        {
            return new LatheManufactureOrder
            {
                Id = this.Id,
                Name = this.Name,
                POReference = this.POReference,
                CreatedAt = this.CreatedAt,
                CreatedBy = this.CreatedBy,
                ModifiedAt = this.ModifiedAt,
                ModifiedBy = this.ModifiedBy,
                TimeToComplete = this.TimeToComplete,
                IsComplete = this.IsComplete,
                Status = this.Status,
                Notes = this.Notes,
                IsUrgent = this.IsUrgent,
                SettingTime = this.SettingTime,
                AllocatedMachine = this.AllocatedMachine,
                AllocatedSetter = this.AllocatedSetter,
                StartDate = this.StartDate,
                SettingFinished = this.SettingFinished,
                CompletedAt = this.CompletedAt,
                IsReady = this.IsReady,
                HasProgram = this.HasProgram,
                HasStarted = this.HasStarted,
                BarID = this.BarID,
                NumberOfBars = this.NumberOfBars
            };
        }
    }
}
