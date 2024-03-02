using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Products
{
    public class NonTurnedItem : BaseObject, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CycleTime { get; set; }
    }
}
