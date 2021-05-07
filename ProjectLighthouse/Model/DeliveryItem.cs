using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class DeliveryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AllocatedDeliveryNote { get; set; }
        public string PurchaseOrderReference { get; set; }
        public string Product { get; set; }
        public int QuantityThisDelivery { get; set; }
        public int QuantityToFollow { get; set; }
    }
}
