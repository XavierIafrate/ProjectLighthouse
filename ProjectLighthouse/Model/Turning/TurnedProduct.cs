using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class TurnedProduct
    {
        private const double MaxDiameter = 38;
        private const double MaxLength = 150;

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        public string ProductName { get; set; }

        public int CycleTime { get; set; }
        public string Material { get; set; }
        public string BarID { get; set; }

        public double MajorLength { get; set; }
        public double MajorDiameter { get; set; }
        public DateTime lastManufactured { get; set; }
        public int SellPrice { get; set; }
        public int QuantitySold { get; set; }
        public int NumberOfOrders { get; set; }
        public int QuantityInStock { get; set; }
        public int QuantityOnPO { get; set; }
        public int QuantityOnSO { get; set; }

        public string ThreadSize { get; set; }
        public string DriveType { get; set; }
        public string DriveSize { get; set; }
        public string ProductGroup { get; set; }
        public int QuantityManufactured { get; set; }

        public bool isSpecialPart { get; set; }
        public string CustomerRef { get; set; }
        public string DrawingFilePath { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }

        public int GetRecommendedQuantity()
        {
            const int targetMonthsStock = 12;
            double scaleFactor = Convert.ToDouble(targetMonthsStock) / 18;
            double toMake = Math.Max((QuantitySold * scaleFactor) - QuantityInStock, 0);

            return Convert.ToInt32(Math.Round(toMake / 100, 0) * 100);
        }

        public TimeSpan GetTimeToMake(int quantity)
        {
            return CycleTime == 0
                ? TimeSpan.FromSeconds(quantity * 120)
                : TimeSpan.FromSeconds(quantity * CycleTime);
        }

        public bool CanBeManufactured()
        {
            return MajorLength <= MaxLength && MajorDiameter <= MaxDiameter;
        }

        public string GetReasonCannotBeMade()
        {
            string reason = "";
            if (MajorDiameter > MaxDiameter)
            {
                reason += "Diameter too large";
            }
            if (MajorLength > MaxLength)
            {
                if (reason == "")
                {
                    reason += "Too long";
                }
                else
                {
                    reason += Environment.NewLine + "Too long";
                }
            }
            return reason;
        }

        public bool IsScheduleCompatible(TurnedProduct otherProduct)
        {
            return
                otherProduct.MajorDiameter == MajorDiameter &&
                otherProduct.DriveSize == DriveSize &&
                otherProduct.DriveType == DriveType &&
                otherProduct.ThreadSize == ThreadSize &&
                otherProduct.ProductGroup == ProductGroup &&
                otherProduct.Material == Material &&
                otherProduct.ProductName != ProductName;
        }

        // For requests engine
        [Ignore]
        public bool RecentlyDeclined { get; set; }
        [Ignore]
        public string OrderReference { get; set; }
        [Ignore]
        public bool IsAlreadyOnOrder { get; set; }
        [Ignore]
        public int QuantityOnOrder { get; set; }
    }
}