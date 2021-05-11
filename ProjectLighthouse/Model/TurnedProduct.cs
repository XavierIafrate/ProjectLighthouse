using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class TurnedProduct
    {
        private const double MaxDiameter = (double)20;
        private const double MaxLength = (double)90;

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public int CycleTime { get; set; }
        public bool isABitchToMake { get; set; }
        public string Material { get; set; }
        public string BarID { get; set; }

        public double MajorLength { get; set; }
        public double MajorDiameter { get; set; }
        public DateTime lastManufactured { get; set; }
        public int SellPrice { get; set; }
        public int QuantitySold { get; set; }
        public int NumberOfOrders { get; set; }
        public int QuantityInStock { get; set; }

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
            double scaleFactor = Convert.ToDouble(targetMonthsStock) / (double)18;
            double toMake = Math.Max(QuantitySold * scaleFactor - QuantityInStock, 0);

            return Convert.ToInt32(Math.Round(toMake / (double)100, 0) * (double)100);
        }

        public bool canBeManufactured()
        {
            return (MajorLength <= MaxLength && MajorDiameter <= MaxDiameter);
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
            return (
                otherProduct.MajorDiameter == MajorDiameter &&
                otherProduct.DriveSize == DriveSize &&
                otherProduct.DriveType == DriveType &&
                otherProduct.ThreadSize == ThreadSize &&
                otherProduct.ProductGroup == ProductGroup &&
                otherProduct.Material == Material
                );
        }
    }
}
