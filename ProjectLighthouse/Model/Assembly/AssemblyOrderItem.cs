using SQLite;

namespace ProjectLighthouse.Model.Assembly
{
    public class AssemblyOrderItem
    {
        [AutoIncrement, PrimaryKey]
        public int ID { get; set; }
        public string OrderReference { get; set; }
        public string ProductName { get; set; }
        public int QuantityRequired { get; set; }
        public int QuantityReady { get; set; }
        public string ChildOf { get; set; }
    }
}
