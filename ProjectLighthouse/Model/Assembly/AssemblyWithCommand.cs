using System.Collections.Generic;


namespace ProjectLighthouse.Model.Assembly
{
    public class AssemblyWithCommand
    {
        public AssemblyOrderItem Parent { get; set; }
        public List<AssemblyItemWithCommands> ChildDataSet { get; set; }
    }
}
