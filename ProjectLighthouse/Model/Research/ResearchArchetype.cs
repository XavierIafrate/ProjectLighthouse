using ProjectLighthouse.Model.Core;
using SQLite;

namespace Model.Research
{
    public class ResearchArchetype : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }    
        public int ProjectId { get; set; }
    }
}
