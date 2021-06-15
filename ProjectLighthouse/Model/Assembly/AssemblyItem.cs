using SQLite;

namespace ProjectLighthouse.Model
{
    public class AssemblyItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductNumber { get; set; }
        public string ProductGroup { get; set; }
        public string Description { get; set; }
        public string BillOfMaterials { get; set; }
        public string Routing { get; set; }
        public int QuantityInStock { get; set; }
        public string LocationOfStock { get; set; }
        public string Units { get; set; }

        public AssemblyItem Clone()
        {
            return new AssemblyItem()
            {
                ID = ID,
                ProductNumber = ProductNumber,
                ProductGroup = ProductGroup,
                Description = Description,
                BillOfMaterials = BillOfMaterials,
                Routing = Routing,
                QuantityInStock = QuantityInStock,
                LocationOfStock = LocationOfStock,
                Units = Units
            };

        }
    }
}
