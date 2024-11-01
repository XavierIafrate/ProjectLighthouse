using SQLite;
using System;

namespace ProjectLighthouse.Model.Deliveries
{
    public class DeliveryItem : BaseObject, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AllocatedDeliveryNote { get; set; }
        public string ItemManufactureOrderNumber { get; set; }

        private string purchaseOrderReference;
        public string PurchaseOrderReference
        {
            get { return purchaseOrderReference; }
            set { purchaseOrderReference = value; OnPropertyChanged(); }
        }

        public string Product { get; set; }


        private string? exportProductName;
        public string? ExportProductName
        {
            get { return exportProductName; }
            set { exportProductName = value; OnPropertyChanged(); }
        }

        public int QuantityThisDelivery { get; set; }
        public int LotID { get; set; }
        [Ignore]
        public string FromMachine { get; set; }

        private string editedBy;
        public string EditedBy
        {
            get { return editedBy; }
            set { editedBy = value; OnPropertyChanged(); }
        }

        private DateTime editedAt;
        public DateTime EditedAt
        {
            get { return editedAt; }
            set { editedAt = value; OnPropertyChanged(); }
        }



        public object Clone()
        {
            return new DeliveryItem()
            {
                Id = this.Id,
                AllocatedDeliveryNote = this.AllocatedDeliveryNote,
                ItemManufactureOrderNumber = this.ItemManufactureOrderNumber,
                PurchaseOrderReference = this.PurchaseOrderReference,
                Product = this.Product,
                QuantityThisDelivery = this.QuantityThisDelivery,
                LotID = this.LotID,
                FromMachine = this.FromMachine,
                ExportProductName = this.ExportProductName,
                EditedBy= this.EditedBy,
                EditedAt = this.EditedAt,
            };
        }
    }
}
