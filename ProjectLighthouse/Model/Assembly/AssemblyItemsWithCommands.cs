using ProjectLighthouse.ViewModel.Commands;

namespace ProjectLighthouse.Model.Assembly
{
    public class AssemblyItemWithCommands
    {
        public AssemblyOrderItem Child { get; set; }
        public UpdateAssemblyOrderItemCommand UpdateCommand { get; set; }
    }
}
