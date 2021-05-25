using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Assembly
{
    public class CompleteAssemblyProduct
    {
        public AssemblyItem product { get; set; }
        public Routings routings { get; set; }
        public BillOfMaterials materials { get; set; }
    }
}
