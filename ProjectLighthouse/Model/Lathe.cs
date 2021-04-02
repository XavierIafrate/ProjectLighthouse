using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class Lathe
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string IPAddress { get; set; }
        public string ControllerReference { get; set; }
        public int MaxDiameter { get; set; }
        public int MaxLength { get; set; }
    }
}
