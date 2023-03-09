using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Requests;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class TurnedProduct : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation, ICloneable
    {
        // TODO refactor to network constants or calculated
        public const double MaxDiameter = 38;
        public const double MaxLength = 150;

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

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

        private string exportProductName;

        [Import("Delivery Name")]
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

        // TODO review
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
            throw new NotImplementedException();
        }


        #endregion


        #region Regular Properties
        [Import("Product Group")]
        public int? GroupId { get; set; }
        [Import("Material")]
        public int? MaterialId { get; set; }
        public bool IsSpecialPart { get; set; }


        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }

        public DateTime? LastManufactured { get; set; }

        [Import("Target Stock")]
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

        public int GetRecommendedQuantity()
        {
            int toMake = (int)Math.Max(QuantitySold - QuantityInStock + QuantityOnSO - QuantityOnPO, 0);

            return RequestsEngine.RoundQuantity(toMake, roundUp:true);
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
            return this.MemberwiseClone();
        }

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