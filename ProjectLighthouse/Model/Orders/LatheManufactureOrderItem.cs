using ProjectLighthouse.Model.Products;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Orders
{
    public class LatheManufactureOrderItem : BaseObject, IObjectWithValidation
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
        public int RecommendedQuantity;
        public int SellPrice;

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public Action<LatheManufactureOrderItem> RequestToEdit { get; set; }

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

        public LatheManufactureOrderItem(TurnedProduct fromProduct)
        {
            ProductName = fromProduct.ProductName;
            ProductId = fromProduct.Id;
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            PartOffLength= fromProduct.PartOffLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.IsSpecialPart;

            RequiredQuantity = 0;
            TargetQuantity = fromProduct.GetRecommendedQuantity();
            SellPrice = fromProduct.SellPrice;
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity)
        {
            ProductName = fromProduct.ProductName;
            ProductId = fromProduct.Id;
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            PartOffLength= fromProduct.PartOffLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.IsSpecialPart;

            RequiredQuantity = requiredQuantity;
            TargetQuantity = fromProduct.GetRecommendedQuantity();
            SellPrice = fromProduct.SellPrice;
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity, DateTime dateRequired)
        {
            ProductName = fromProduct.ProductName;
            ProductId = fromProduct.Id;
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            PartOffLength= fromProduct.PartOffLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.IsSpecialPart;

            RequiredQuantity = requiredQuantity;
            TargetQuantity = Math.Max(fromProduct.GetRecommendedQuantity(), requiredQuantity);
            DateRequired = dateRequired;
            SellPrice = fromProduct.SellPrice;

        }

        public override string ToString()
        {
            return ProductName;
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
                    <= 5 => 90,
                    <= 7 => 100,
                    <= 10 => 120,
                    <= 15 => 180,
                    <= 20 => 240,
                    <= 25 => 270,
                    _ => 320
                };
            }
        }

        public int GetTimeToMakeRequired()
        {
            return GetCycleTime() * RequiredQuantity;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(TargetQuantity));
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

                if(TargetQuantity < RequiredQuantity)
                {
                    AddError(propertyName, "Target Quantity must be greater than or equal to Required Quantity");
                    return;
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
