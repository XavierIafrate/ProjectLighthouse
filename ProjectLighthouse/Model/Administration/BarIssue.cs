using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Administration
{
    public class BarIssue
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string IssuedBy { get; set; }
        public string BarId { get; set; }
        public string OrderId { get; set; }
        public string MaterialBatch { get; set; }
        public int Quantity { get; set; }
    }
}
