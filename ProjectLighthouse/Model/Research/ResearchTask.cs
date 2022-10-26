using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Research
{
    public class ResearchTask : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public ResearchStage Stage { get; set; }
        public bool Complete { get; set; }
        public DateTime ModifiedAt { get; set;  }
        public string ModifiedBy { get; set; }
        public string TaskName { get; set; }
    }
}
