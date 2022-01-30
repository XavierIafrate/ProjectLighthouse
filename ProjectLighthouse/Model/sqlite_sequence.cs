using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class sqlite_sequence
    {
        [PrimaryKey]
        public string name { get; set; }
        public int seq { get; set; }
    }
}
