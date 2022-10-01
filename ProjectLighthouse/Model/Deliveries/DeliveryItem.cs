using SQLite;
using System;

namespace ProjectLighthouse.Model.Deliveries
{
    public class DeliveryItem : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AllocatedDeliveryNote { get; set; }
        public string ItemManufactureOrderNumber { get; set; }
        public string PurchaseOrderReference { get; set; }
        public string Product { get; set; }
        public int QuantityThisDelivery { get; set; }
        public int QuantityToFollow { get; set; }
        public int LotID { get; set; }
        [Ignore]
        public string FromMachine { get; set; }

        public object Clone()
        {
            return new DeliveryItem()
            {
                Id = Id,
                AllocatedDeliveryNote = AllocatedDeliveryNote,
                ItemManufactureOrderNumber = ItemManufactureOrderNumber,
                PurchaseOrderReference = PurchaseOrderReference,
                Product = Product,
                QuantityThisDelivery = QuantityThisDelivery,
                QuantityToFollow = QuantityToFollow,
                LotID = LotID,
                FromMachine = FromMachine,
            };

        }
    }
}
