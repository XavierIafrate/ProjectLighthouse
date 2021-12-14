using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public partial class LatheManufactureOrder : ICloneable
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

        public OrderState State
        {
            get
            {
                if (IsCancelled)
                {
                    return OrderState.Cancelled;
                }
                else if (IsComplete)
                {
                    return OrderState.Complete;
                }
                else if (HasStarted)
                {
                    return OrderState.Running;
                }
                else if (BarIsAllocated && BarIsVerified && HasProgram && IsReady)
                {
                    return OrderState.Prepared;
                }
                else if (BarIsVerified && HasProgram && IsReady && StartDate.AddDays(-14) > DateTime.Now)
                {
                    return OrderState.Ready;
                }
                else
                {
                    return OrderState.Problem;
                }
            }
            set { }
        }

        public bool IsReady { get; set; } // alias for tooling ready (legacy)
        public bool HasProgram { get; set; }
        public bool HasStarted { get; set; }
        public bool BarIsAllocated { get; set; }
        public bool BarIsVerified { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }

        public string Status { get; set; }

        public bool IsUrgent { get; set; }
        public string AllocatedMachine { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime SettingFinished { get; set; }
        public DateTime CompletedAt { get; set; }
        public string BarID { get; set; }
        public double NumberOfBars { get; set; }
        public double BarsInStockAtCreation { get; set; }
        public double MajorDiameter { get; set; }
        public bool ItemNeedsCleaning { get; set; }

        public object Clone()
        {
            return new LatheManufactureOrder
            {
                Id = Id,
                Name = Name,
                POReference = POReference,
                CreatedAt = CreatedAt,
                CreatedBy = CreatedBy,
                ModifiedAt = ModifiedAt,
                ModifiedBy = ModifiedBy,
                TimeToComplete = TimeToComplete,
                IsComplete = IsComplete,
                Status = Status,
                IsUrgent = IsUrgent,
                IsCancelled = IsCancelled,
                State = State,
                AllocatedMachine = AllocatedMachine,
                StartDate = StartDate,
                SettingFinished = SettingFinished,
                CompletedAt = CompletedAt,
                IsReady = IsReady,
                HasProgram = HasProgram,
                HasStarted = HasStarted,
                BarID = BarID,
                NumberOfBars = NumberOfBars,
                ItemNeedsCleaning = ItemNeedsCleaning,
                BarIsAllocated = BarIsAllocated,
                BarIsVerified = BarIsVerified,
                BarsInStockAtCreation = BarsInStockAtCreation
            };
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsUpdated(LatheManufactureOrder OtherOrder)
        {
            return IsCancelled != OtherOrder.IsCancelled
                || IsComplete != OtherOrder.IsComplete
                || HasStarted != OtherOrder.HasStarted
                || BarIsAllocated != OtherOrder.BarIsAllocated
                || BarIsVerified != OtherOrder.BarIsVerified
                || HasProgram != OtherOrder.HasProgram
                || IsReady != OtherOrder.IsReady
                || POReference != OtherOrder.POReference
                || NumberOfBars != OtherOrder.NumberOfBars
                || TimeToComplete != OtherOrder.TimeToComplete;
        }
    }
}
