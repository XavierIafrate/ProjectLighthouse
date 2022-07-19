using ProjectLighthouse.ViewModel;
using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class TurnedProduct
    {
        public const double MaxDiameter = 38;
        public const double MaxLength = 150;

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        public string ProductName { get; set; }

        public int CycleTime { get; set; }
        public string Material { get; set; }
        public string BarID { get; set; }

        public double MajorLength { get; set; }
        public double PartOffLength { get; set; }
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
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }

        public string SpecificationDocument { get; set; }
        public string SpecificationDetails { get; set; }

        public bool Retired { get; set; }

        [Ignore]
        public Product Group { get; set; }
        [Ignore]
        public int TargetStock
        {
            get { return GetRecommendedQuantity(); }
        }

        public int GetCycleTime()
        {
            if (CycleTime != 0)
            {
                return CycleTime;
            }
            else
            {
                return MajorDiameter switch
                {
                    < 15 => 120,
                    < 24 => 180,
                    _ => 240
                };
            }
        }

        public int GetRecommendedQuantity(bool forManufacture = false)
        {
            const int targetMonthsStock = 12;
            double scaleFactor = Convert.ToDouble(targetMonthsStock) / 18;
            double toMake = Math.Max((QuantitySold * scaleFactor) - QuantityInStock, 0);

            int qty = Convert.ToInt32(Math.Round(toMake / 100, 0) * 100);

            return forManufacture 
                ? Math.Max(qty, RequestsEngine.GetMiniumumOrderQuantity(this)) 
                : qty;
        }

        public TimeSpan GetTimeToMake(int quantity)
        {
            return TimeSpan.FromSeconds(quantity * GetCycleTime());
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
                otherProduct.Material == Material;
        }

        public bool DataIsComplete()
        {
            return MajorDiameter > 0
                && MajorLength > 0
                && !string.IsNullOrWhiteSpace(ProductName)
                && !string.IsNullOrWhiteSpace(ProductGroup)
                && !string.IsNullOrWhiteSpace(Material)
                && !string.IsNullOrWhiteSpace(BarID)
                && !string.IsNullOrWhiteSpace(ThreadSize)
                && !string.IsNullOrWhiteSpace(DriveSize)
                && !string.IsNullOrWhiteSpace(DriveType);

        }

        // For requests engine
        [Ignore]
        public Request DeclinedRequest { get; set; }
        [Ignore]
        public LatheManufactureOrder AppendableOrder { get; set; }
        [Ignore]
        public LatheManufactureOrder ZeroSetOrder { get; set; }
        [Ignore]
        public int LighthouseGuaranteedQuantity { get; set; }

        public int FreeStock()
        {
            return QuantityInStock + LighthouseGuaranteedQuantity - QuantityOnSO;
        }
    }
}