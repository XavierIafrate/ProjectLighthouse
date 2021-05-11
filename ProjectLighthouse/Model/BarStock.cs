using SQLite;

namespace ProjectLighthouse.Model
{
    public class BarStock
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Material { get; set; }
        public string Form { get; set; }
        public int Length { get; set; }
        public int Size { get; set; }
        public double InStock { get; set; }
        public int Cost { get; set; }
    }
}
