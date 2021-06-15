using SQLite;

namespace ProjectLighthouse.Model.Assembly
{
    public class BillOfMaterialsItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string BOMID { get; set; }
        public string ForProduct { get; set; }
        public string ComponentItem { get; set; }
        public int Quantity { get; set; }
        public int UnitCost { get; set; }
        public string Units { get; set; }
    }
}
