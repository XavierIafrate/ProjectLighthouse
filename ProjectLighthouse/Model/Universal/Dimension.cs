using SQLite;

namespace ProjectLighthouse.Model.Universal
{
    public class Dimension
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Product { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
