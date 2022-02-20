using SQLite;
using System;
using CsvHelper;

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
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.isSpecialPart;

            RequiredQuantity = 0;
            TargetQuantity = fromProduct.GetRecommendedQuantity( forManufacture:true );
            RecommendedQuantity = TargetQuantity;
            SellPrice = fromProduct.SellPrice;
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity)
        {
            ProductName = fromProduct.ProductName;
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.isSpecialPart;

            RequiredQuantity = requiredQuantity;
            TargetQuantity = requiredQuantity + fromProduct.GetRecommendedQuantity(forManufacture: true);
            RecommendedQuantity = TargetQuantity - RequiredQuantity;
            SellPrice = fromProduct.SellPrice;

        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity, DateTime dateRequired, bool needsCleaning = false)
        {
            ProductName = fromProduct.ProductName;
            CycleTime = fromProduct.CycleTime;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.isSpecialPart;

            RequiredQuantity = requiredQuantity;
            TargetQuantity = requiredQuantity + fromProduct.GetRecommendedQuantity(forManufacture: true);
            DateRequired = dateRequired;
            RecommendedQuantity = TargetQuantity - RequiredQuantity;
            SellPrice = fromProduct.SellPrice;

        }

        public override string ToString()
        {
            return ProductName;
        }
    }
}
