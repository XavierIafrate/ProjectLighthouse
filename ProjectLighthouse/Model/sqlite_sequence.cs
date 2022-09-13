using SQLite;

namespace ProjectLighthouse.Model
{
    public class sqlite_sequence
    {
        [PrimaryKey]
        public string name { get; set; }
        public int seq { get; set; }
    }
}
