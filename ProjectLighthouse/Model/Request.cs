using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class Request
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Product { get; set; }
        public string POReference { get; set; }
        public int QuantityRequired { get; set; }
        public DateTime DateRequired { get; set; }
        public DateTime DateRaised { get; set; }
        public bool isSchedulingApproved { get; set; }
        public bool isProductionApproved { get; set; }
        public bool IsDeclined { get; set; }
        public bool IsAccepted { get; set; }
        public string AcceptedBy { get;  set; }
        public string RaisedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string DeclinedReason { get; set; }
        public string ResultingLMO { get; set; }
        public string Status { get; set; }
        public string Likeliness { get; set; }
        public string Notes { get; set; }
    }
}
