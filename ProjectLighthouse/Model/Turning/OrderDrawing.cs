using SQLite;

namespace ProjectLighthouse.Model
{
    public class OrderDrawing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int DrawingId { get; set; }
    }
}
