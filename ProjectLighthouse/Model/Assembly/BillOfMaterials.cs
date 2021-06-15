using System.Collections.Generic;

namespace ProjectLighthouse.Model.Assembly
{
    public class BillOfMaterials
    {
        public string ID { get; set; }
        public string ToMake { get; set; }
        public List<BillOfMaterialsItem> Items { get; set; }
    }
}
