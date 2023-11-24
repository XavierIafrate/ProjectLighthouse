using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Quality
{
    public class Standard : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, NotNull]
        public string Name { get; set; }
        [NotNull]
        public string Description { get; set; }
    }
}
