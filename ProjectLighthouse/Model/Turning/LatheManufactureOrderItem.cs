using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class LatheManufactureOrderItem
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public string AssignedMO { get; set; }
        [NotNull]
        public string ProductName { get; set; }
        public int RequiredQuantity { get; set; }
        [NotNull]
        public int TargetQuantity { get; set; }
        public int QuantityMade { get; set; }
        public int QuantityReject { get; set; }
        public int QuantityDelivered { get; set; }
        public int CycleTime { get; set; }
        public double MajorLength { get; set; }

        public DateTime DateRequired { get; set; }
        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
        public bool IsSpecialPart { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
