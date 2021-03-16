using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{

    public class PostProcess
    {
        public string ProcessName { get; set; }
    }
    public class TurnedProduct
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public int CycleTime { get; set; }
        public bool isABitchToMake { get; set; }
        public string Material { get; set; }
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

        public int GetRecommendedQuantity()
        {
            const int targetMonthsStock = 12;
            int recommendedQuantity = 0;
            int soldPerMonth = QuantitySold / 18;
            double scaleFactor = Convert.ToDouble(targetMonthsStock) / (double)18;
            double toMake = QuantitySold * scaleFactor - QuantityInStock;

            recommendedQuantity = Convert.ToInt32( Math.Round( toMake / (double)100, 0) * (double)100);
            

            return recommendedQuantity;
        }

    }
}
