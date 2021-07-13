using System.Collections.Generic;

namespace ProjectLighthouse.Model.Assembly
{
    public class Assembly
    {
        public AssemblyOrderItem Parent { get; set; }
        public List<AssemblyOrderItem> Children { get; set; }
    }
}
