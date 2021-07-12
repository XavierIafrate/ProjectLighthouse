using SQLite;

namespace ProjectLighthouse.Model
{
    public class DeliveryItem
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
    }
}
