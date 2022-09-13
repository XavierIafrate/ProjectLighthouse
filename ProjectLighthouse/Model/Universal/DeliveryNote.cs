using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class DeliveryNote
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string Name { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveredBy { get; set; }
        public bool Verified { get; set; }
    }
}
