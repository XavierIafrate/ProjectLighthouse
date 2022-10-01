using SQLite;

namespace ProjectLighthouse.Model.Core
{
    public interface IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
