using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Products
{
    public class ProductGroup : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }


        public int ProductId { get; set; }
        public double? MinBarSize { get; set; }
        public double MajorDiameter { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
