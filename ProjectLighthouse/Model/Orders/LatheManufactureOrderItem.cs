using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Requests;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Orders
{
    public class LatheManufactureOrderItem : BaseObject, IObjectWithValidation, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public string AssignedMO { get; set; }
        [NotNull]
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        private int requiredQuantity;

        public int RequiredQuantity
        {
            get { return requiredQuantity; }
            set
            {
                requiredQuantity = value;
                ValidateProperty(nameof(TargetQuantity));
                OnPropertyChanged();
            }
        }


        private int targetQuantity;
        [NotNull]
        public int TargetQuantity
        {
            get { return targetQuantity; }
            set
            {
                targetQuantity = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public int QuantityMade { get; set; }
        public int QuantityReject { get; set; }
        public int QuantityDelivered { get; set; }
        public int CycleTime { get; set; }
        public int? PreviousCycleTime { get; set; }

        private int? modelledCycleTime;
        public int? ModelledCycleTime
        {
            get { return modelledCycleTime; }
            set { modelledCycleTime = value; OnPropertyChanged(); }
        }


        private double majorLength;
        public double MajorLength
        {
            get { return majorLength; }
            set
            {
                if (value == majorLength) return;
                majorLength = value;
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

        public double MajorDiameter { get; set; }

        public int DrawingId { get; set; }

        public DateTime DateRequired { get; set; }
        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
        public bool IsSpecialPart { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool NeedsCleaning { get; set; }

        public bool ShowEdit;
        public int SellPrice;

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public Action<LatheManufactureOrderItem> RequestToEdit { get; set; }


        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public int RecommendedQuantity { get; set; }

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public int QuantityInStock { get; set; }
        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public int YearStock { get; set; }

        public void NotifyRequestToEdit()
        {
            RequestToEdit?.Invoke(this);
        }

        public event Action EditMade;
        public void NotifyEditMade()
        {
            EditMade?.Invoke();
        }

        public LatheManufactureOrderItem()
        {

        }

        private void SetupFromProduct(TurnedProduct fromProduct)
        {
            ProductName = fromProduct.ProductName;
            ProductId = fromProduct.Id;
            PreviousCycleTime = fromProduct.CycleTime > 0 ? fromProduct.CycleTime : null;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            PartOffLength = fromProduct.PartOffLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.IsSpecialPart;

            RequiredQuantity = 0;
            TargetQuantity = Math.Max(fromProduct.GetRecommendedQuantity(), 1);
            SellPrice = fromProduct.SellPrice;

            QuantityInStock = fromProduct.QuantityInStock;
            RecommendedQuantity = fromProduct.GetRecommendedQuantity();
            YearStock = fromProduct.QuantitySold;
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct)
        {
            SetupFromProduct(fromProduct);
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity)
        {
            SetupFromProduct(fromProduct);

            RequiredQuantity = requiredQuantity;
            TargetQuantity = RequestsEngine.RoundQuantity(Math.Max(TargetQuantity, RequiredQuantity), roundUp: true);
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity, DateTime dateRequired)
        {
            SetupFromProduct(fromProduct);

            DateRequired = dateRequired;
            RequiredQuantity = requiredQuantity;
            TargetQuantity = RequestsEngine.RoundQuantity(Math.Max(TargetQuantity, RequiredQuantity), roundUp: true);
        }

        public override string ToString()
        {
            return ProductName;
        }

        public int PlannedCycleTime()
        {
            return PreviousCycleTime ?? ModelledCycleTime ?? RequestsEngine.EstimateCycleTime(MajorDiameter);
        }

        public int GetTimeToMakeRequired()
        {
            return (CycleTime == 0 ? PlannedCycleTime() : CycleTime) * RequiredQuantity;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(RequiredQuantity));
            ValidateProperty(nameof(TargetQuantity));
            ValidateProperty(nameof(MajorLength));
            ValidateProperty(nameof(PartOffLength));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(TargetQuantity))
            {
                ClearErrors(propertyName);

                if (TargetQuantity <= 0)
                {
                    AddError(propertyName, "Target Quantity must be greater than zero");
                    return;
                }

                if (TargetQuantity < RequiredQuantity)
                {
                    AddError(propertyName, "Target Quantity must be greater than or equal to Required Quantity");
                    return;
                }

                if (TargetQuantity > 100_000)
                {
                    AddError(propertyName, "Max Target Quantity is 100,000");
                }

                return;
            }
            else if (propertyName == nameof(RequiredQuantity))
            {
                ClearErrors(propertyName);

                if (TargetQuantity < 0)
                {
                    AddError(propertyName, "Required Quantity must be greater than or equal to zero");
                    return;
                }

                if (RequiredQuantity > 100_000)
                {
                    AddError(propertyName, "Max Required Quantity is 100,000");
                }

                return;
            }
            else if (propertyName == nameof(MajorLength))
            {
                ClearErrors(propertyName);

                if (MajorLength <= 0)
                {
                    AddError(propertyName, "Major Length must be greater than zero");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(PartOffLength))
            {
                ClearErrors(propertyName);

                if (PartOffLength < 0)
                {
                    AddError(propertyName, "Extra Material must be greater than or equal to zero");
                    return;
                }

                return;
            }

            throw new NotImplementedException();
        }
    }
}
