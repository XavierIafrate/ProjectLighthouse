using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Assembly
{
    class BillOfMaterials
    {
        public int ID { get; set; }
        public List<BillOfMaterialsItem> Items { get; set; }
    }
}
