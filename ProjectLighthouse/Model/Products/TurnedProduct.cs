using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Requests;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class TurnedProduct : Item, IObjectWithValidation, ICloneable
    {
        
        #region Full Members
        private string productName;
        [Indexed]
        [Import("Item Name")]
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

        private string? exportProductName;

        [Unique]
        [Import("Delivery Name")]
        public string? ExportProductName
        {
            get { return exportProductName; }
            set
            {
                exportProductName = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private bool isSyncing;
        [Import("Is Syncing")]
        public bool IsSyncing
        {
            get { return isSyncing; }
            set
            {
                if (isSyncing == value) return;

                isSyncing = value;
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
        [Import("Major Length")]
        public double MajorLength
        {
            get { return majorLength; }
            set
            {
                if (majorLength == value) return;

                majorLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double majorDiameter;
        [Import("Major Diameter")]
        public double MajorDiameter
        {
            get { return majorDiameter; }
            set
            {
                if (majorDiameter == value) return;
                majorDiameter = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double partOffLength;
        [Import("Extra Material")]
        public double PartOffLength
        {
            get { return partOffLength; }
            set
            {
                if (partOffLength == value) return;
                partOffLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private bool platedPart;
        public bool PlatedPart
        {
            get { return platedPart; }
            set { platedPart = value; OnPropertyChanged(); }
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
            ValidateProperty(nameof(GroupId));
            ValidateProperty(nameof(MaterialId));
            ValidateProperty(nameof(QuantitySold));
        }

        public void ValidateForOrder()
        {
            ValidateProperty(nameof(MajorLength));
            ValidateProperty(nameof(MajorDiameter));
            ValidateProperty(nameof(GroupId));
            ValidateProperty(nameof(MaterialId));
        }

        public void ValidateForRequest()
        {
            ValidateProperty(nameof(ProductName));
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

                if (!ValidationHelper.IsValidProductName(ProductName))
                {
                    string illegalChars = ValidationHelper.GetInvalidProductNameChars(ProductName);
                    AddError(propertyName, $"Id must be a valid product code (illegal character(s): {illegalChars})");
                    return;
                }

                if (ProductName.ToUpperInvariant() == "ORDER")
                {
                    AddError(nameof(ProductName), "Product Name cannot be 'Order'");
                    return;
                }

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
                if (CycleTime < 0)
                {
                    AddError(nameof(CycleTime), "Cycle Time must be greater than zero");
                }
                return;
            }
            else if (propertyName == nameof(MajorLength))
            {
                ClearErrors(propertyName);
                if (MajorLength <= 0)
                {
                    AddError(nameof(MajorLength), "Major length must be greater than zero");
                }

                if (MajorLength > 25_000)
                {
                    AddError(nameof(MajorLength), "In 2022 the largest lathe had a max workpiece length of 25,000mm");
                }
                return;
            }
            else if (propertyName == nameof(MajorDiameter))
            {
                ClearErrors(propertyName);
                if (MajorDiameter <= 0)
                {
                    AddError(nameof(MajorDiameter), "Major diameter must be greater than zero");
                }

                if (MajorDiameter > 7_000)
                {
                    AddError(nameof(MajorDiameter), "In 2022 the largest lathe had a max turning diameter of 7,000mm");
                }

                return;
            }
            else if (propertyName == nameof(PartOffLength))
            {
                ClearErrors(propertyName);
                if (PartOffLength < 0)
                {
                    AddError(nameof(PartOffLength), "Part off length cannot be less than zero");
                }
                return;
            }
            else if (propertyName == nameof(GroupId))
            {
                ClearErrors(propertyName);
                if (GroupId is null)
                {
                    AddError(nameof(GroupId), "The product must have a group associated with it");
                    return;
                }

                if (GroupId == 0)
                {
                    AddError(nameof(GroupId), "The product must have a group associated with it");
                }

                if (GroupId == -1)
                {
                    AddError(nameof(GroupId), "The product must have a group associated with it");
                }
                return;
            }
            else if (propertyName == nameof(MaterialId))
            {
                ClearErrors(propertyName);
                if (MaterialId is null)
                {
                    AddError(nameof(MaterialId), "The product must have a material associated with it");
                    return;
                }

                if (MaterialId == 0)
                {
                    AddError(nameof(MaterialId), "The product must have a material associated with it");
                }
                return;
            }
            else if (propertyName == nameof(QuantitySold))
            {
                ClearErrors(propertyName);
                if (QuantitySold < 0)
                {
                    AddError(nameof(QuantitySold), "Target stock must be greater than or equal to zero");
                    return;
                }

                if (QuantitySold < 0)
                {
                    AddError(nameof(QuantitySold), "Target stock must be greater than or equal to zero");
                    return;
                }
                return;
            }
            throw new NotImplementedException();
        }


        #endregion


        #region Regular Properties
        [Import("Product Group")]
        public int? GroupId { get; set; }
        [Import("Material")]
        public int? MaterialId { get; set; }
        public bool IsSpecialPart { get; set; }



        [Import("Target Stock")]
        public int QuantitySold { get; set; }
        public int NumberOfOrders { get; set; }
        public int SellPrice { get; set; }

        public int QuantityManufactured { get; set; }

        public string SpecificationDocument { get; set; }
        public string SpecificationDetails { get; set; }


        private Cost itemCost;
        [Ignore]
        [Newtonsoft.Json.JsonIgnore]
        public Cost ItemCost
        {
            get { return itemCost; }
            set { itemCost = value; OnPropertyChanged(); }
        }


        #endregion


        #region Helper Functions

        public int GetCycleTime()
        {
            if (CycleTime != 0)
            {
                return CycleTime;
            }

            return RequestsEngine.EstimateCycleTime(MajorDiameter);
        }

        public int GetRecommendedQuantity()
        {
            int toMake = (int)Math.Max(QuantitySold - QuantityInStock + QuantityOnSO - QuantityOnPO, 0);

            return RequestsEngine.RoundQuantity(toMake, roundUp: true);
        }

        public TimeSpan GetTimeToMake(int quantity)
        {
            return TimeSpan.FromSeconds(quantity * GetCycleTime());
        }

        public bool IsScheduleCompatible(TurnedProduct otherProduct)
        {
            if (GroupId is null) return false;
            if (MaterialId is null) return false;

            return otherProduct.GroupId == GroupId && otherProduct.MaterialId == MaterialId;
        }

        public int FreeStock()
        {
            return QuantityInStock + LighthouseGuaranteedQuantity - QuantityOnSO;
        }

        public bool Overstocked()
        {
            if (QuantitySold == 0)
            {
                return QuantityInStock > 200;
            }

            return (double)QuantityInStock / QuantitySold > 2.5;
        }

        #endregion

        public override string ToString()
        {
            return ProductName ?? "NULL";
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TurnedProduct>(serialised);
        }

        #region Scheduling Members
        public LatheManufactureOrder AppendableOrder;
        public LatheManufactureOrder ZeroSetOrder;
        public int LighthouseGuaranteedQuantity;
        #endregion

        public class Cost : BaseObject
        {
            public MaterialInfo Material { get; set; }
            public BarStock BarStock { get; set; }
            public TimeModel TimeModel { get; set; }
            public double Length { get; set; }
            public double BarMass { get; set; }
            public double MaterialBudget { get; set; }
            public double AbsorptionRate { get; set; }
            public int ModelledCycleTime { get; set; }

            private double timeCost;
            public double TimeCost
            {
                get { return timeCost; }
                set { timeCost = value; OnPropertyChanged(); }
            }

            private double materialCost;
            public double MaterialCost
            {
                get { return materialCost; }
                set { materialCost = value; OnPropertyChanged(); }
            }

            private bool estimated;
            public bool Estimated
            {
                get { return estimated; }
                set { estimated = value; OnPropertyChanged(); }
            }

            private double totalCost;
            public double TotalCost
            {
                get { return totalCost; }
                set { totalCost = value; OnPropertyChanged(); }
            }


            public Cost(MaterialInfo materialInfo, BarStock bar, TimeModel model, double length, double materialBudget)
            {
                this.Material = materialInfo;
                this.BarStock = bar;
                this.TimeModel = model;
                this.Length = length;
                this.MaterialBudget = materialBudget;
                this.AbsorptionRate = App.Constants.AbsorptionRate;
                this.ModelledCycleTime = TimeModel.At(Length);

                CalculateCost();
            }

            private void CalculateCost()
            {
                if (this.Material is null) return;
                BarStock.MaterialData = this.Material;
                if (this.Material.Cost is null) return;

                this.BarMass = BarStock.GetUnitMassOfBar();
                MaterialCost = BarMass / BarStock.Length * this.MaterialBudget * ((double)this.Material.Cost / 100);

                TimeCost = App.Constants.AbsorptionRate * TimeModel.At(Length);

                TotalCost = TimeCost + MaterialCost;
            }
        }
    }
}