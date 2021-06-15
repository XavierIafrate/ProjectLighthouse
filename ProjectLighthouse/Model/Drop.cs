using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class Drop
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ForOrder { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public DateTime DateRequired { get; set; }
    }
}
