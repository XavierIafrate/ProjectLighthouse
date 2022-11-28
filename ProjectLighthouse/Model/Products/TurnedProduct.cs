using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.ViewModel.Requests;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class TurnedProduct : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation
    {
        // TODO refactor to network constants or calculated
        public const double MaxDiameter = 38;
        public const double MaxLength = 150;

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        #region Full Members
        private string productName;
        [Indexed]
        public string ProductName
        {
            get { return productName; }
            set 
            { 
                productName = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string exportProductName;

        public string ExportProductName
        {
            get { return exportProductName; }
            set 
            { 
                exportProductName = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int cycleTime;
        public int CycleTime
        {
            get { return cycleTime; }
            set 
            { 
                cycleTime = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double majorLength;
        public double MajorLength
        {
            get { return majorLength; }
            set 
            { 
                majorLength = value; 
                ValidateProperty(); 
                OnPropertyChanged(); 
            }
        }

        // TODO review
        private double majorDiameter;
        public double MajorDiameter
        {
            get { return majorDiameter; }
            set 
            { 
                majorDiameter = value; 
                ValidateProperty(); 
                OnPropertyChanged(); 
            }
        }

        private double partOffLength;
        public double PartOffLength
        {
            get { return partOffLength; }
            set 
            { 
                partOffLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        #endregion

        #region Validation


        public void ValidateAll()
        {
            ValidateProperty(nameof(ProductName));
            ValidateProperty(nameof(ExportProductName));
            ValidateProperty(nameof(CycleTime));
            ValidateProperty(nameof(MajorLength));
            ValidateProperty(nameof(MajorDiameter));
            ValidateProperty(nameof(PartOffLength));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(ProductName))
            {
                ClearErrors(propertyName);
                if (string.IsNullOrEmpty(ProductName))
                {
                    AddError(nameof(ProductName), "Product Name cannot be empty");
                    return;
                }

                // TODO validationhelper
                //if(!Validate)

                return;
            }
            else if (propertyName == nameof(ExportProductName))
            {
                ClearErrors(propertyName);

                return;
            }
            else if (propertyName == nameof(CycleTime))
            {
                ClearErrors(propertyName);
                return;
            }
            else if (propertyName == nameof(MajorLength))
            {
                ClearErrors(propertyName);
                return;
            }
            else if (propertyName == nameof(MajorDiameter))
            {
                ClearErrors(propertyName);
                return;
            }
            else if (propertyName == nameof(PartOffLength))
            {
                ClearErrors(propertyName);
                return;
            }
            throw new NotImplementedException();
        }


        #endregion


        #region Regular Properties
        public int GroupId { get; set; }
        public int MaterialId { get; set; }
        public bool IsSpecialPart { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }


        public DateTime LastManufactured { get; set; }
        public int QuantitySold { get; set; }
        public int NumberOfOrders { get; set; }
        public int SellPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int QuantityOnPO { get; set; }
        public int QuantityOnSO { get; set; }
        public int QuantityManufactured { get; set; }

        public string SpecificationDocument { get; set; }
        public string SpecificationDetails { get; set; }

        public bool Retired { get; set; }
        #endregion


        #region Helper Functions

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
            // TODO refactor to constants
            const int targetMonthsStock = 12;
            double scaleFactor = Convert.ToDouble(targetMonthsStock) / 18;
            double toMake = Math.Max(QuantitySold * scaleFactor - QuantityInStock, 0);

            int qty = Convert.ToInt32(Math.Round(toMake / 100, 0) * 100);

            return forManufacture
                ? Math.Max(qty, RequestsEngine.GetMiniumumOrderQuantity(this))
                : qty;
        }

        public TimeSpan GetTimeToMake(int quantity)
        {
            return TimeSpan.FromSeconds(quantity * GetCycleTime());
        }

        public bool IsScheduleCompatible(TurnedProduct otherProduct)
        {
            return otherProduct.GroupId == GroupId && otherProduct.MaterialId == MaterialId;
        }
        public int FreeStock()
        {
            return QuantityInStock + LighthouseGuaranteedQuantity - QuantityOnSO;
        }

        #endregion

        #region Scheduling Members
        [Ignore]
        public Request DeclinedRequest { get; set; }
        [Ignore]
        public LatheManufactureOrder AppendableOrder { get; set; }
        [Ignore]
        public LatheManufactureOrder ZeroSetOrder { get; set; }
        [Ignore]
        public int LighthouseGuaranteedQuantity { get; set; }
        #endregion

    }
}