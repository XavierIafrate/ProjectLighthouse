using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class AssemblyItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductNumber { get; set; }
        public string BOM { get; set; }
    }
}
