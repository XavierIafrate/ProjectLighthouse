using SQLite;
using System;

namespace ProjectLighthouse.Model.Orders
{
    public class Lot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string Order { get; set; }
        public string AddedBy { get; set; }
        public int Quantity { get; set; }

        public DateTime Date { get; set; }

        public DateTime DateProduced { get; set; }
        public bool IsReject { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsDelivered { get; set; }
        public string MaterialBatch { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string FromMachine { get; set; }
        public string Remarks { get; set; }
        public bool AllowDelivery { get; set; }
        public int CycleTime { get; set; }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Lot>(serialised);
        }
    }
}
