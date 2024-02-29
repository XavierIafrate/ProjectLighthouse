using ProjectLighthouse.Model.Core;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Products
{
    public class Item : BaseObject, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? LastManufactured { get; set; }
        public int QuantityInStock { get; set; }
        public int QuantityOnPO { get; set; }
        public int QuantityOnSO { get; set; }
        public bool Retired { get; set; }
    }
}
