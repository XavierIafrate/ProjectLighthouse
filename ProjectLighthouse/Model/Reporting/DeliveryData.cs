using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Reporting
{
    public class DeliveryData
    {
        public DeliveryNote Header { get; set; }    
        public DeliveryItem[] Lines { get; set; }
    }
}
