using SQLite;
using System;

namespace Model.Research
{
    public class ResearchProject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProductName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        public ResearchStage Stage { get; set; }

        // ProductSizes
    }
}
