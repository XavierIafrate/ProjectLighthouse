using SQLite;

namespace ProjectLighthouse.Model.Orders
{
    public class BreakdownCode
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
