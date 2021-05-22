using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Assembly
{
    public class Routing
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Workstation { get; set; }
        public int SetupTime { get; set; }
        public int CycleTime { get; set; }
    }
}
