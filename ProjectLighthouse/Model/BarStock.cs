using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    class BarStock
    {
        public string Id { get; set; }
        public string Material { get; set; }
        public string Form { get; set; }
        public int Length { get; set; }
        public int Size { get; set; }
        public string VendorId { get; set; }
        public double InStock { get; set; }
    }
}
