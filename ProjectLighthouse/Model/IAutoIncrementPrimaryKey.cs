using SQLite;

namespace ProjectLighthouse.Model
{
    public interface IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
