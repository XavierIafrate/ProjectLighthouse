namespace ProjectLighthouse.Model.Assembly
{
    public class CompleteAssemblyProduct
    {
        public AssemblyItem product { get; set; }
        public Routings routings { get; set; }
        public BillOfMaterials materials { get; set; }
    }
}
