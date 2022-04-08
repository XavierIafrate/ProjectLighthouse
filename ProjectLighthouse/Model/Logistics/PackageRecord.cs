using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Logistics
{
    public class PackageRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private DateTime timeStamp;
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set 
            { 
                timeStamp = value;  
                StrTimeStamp = $"{value:s}"; 
            }
        }

        public string StrTimeStamp { get; set; }

        public string PackedBy { get; set; }
        public string WorkStation { get; set; }
        public string MachineName { get; set; }
        public string DomainUser { get; set; }  
        [NotNull]
        public string OrderReference { get; set; }
        public int NumPackages { get; set; }

    }
}
