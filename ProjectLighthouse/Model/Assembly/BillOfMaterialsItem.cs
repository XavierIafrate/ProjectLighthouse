using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Assembly
{
    public class BillOfMaterialsItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int BOMNumber { get; set; }
        public string ForProduct { get; set; }
        public string ComponentItem { get; set; }
        public string Quantity { get; set; }
        public string Description { get; set; }
        public int UnitCost { get; set; }
        public string Units { get; set; }
    }
}
