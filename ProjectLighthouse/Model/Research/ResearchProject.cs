using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;

namespace Model.Research
{
    public class ResearchProject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        public ResearchStage Stage { get; set; }

        [Ignore]
        public List<Note> Notes { get; set; }
        [Ignore]
        public List<ResearchArchetype> Archetypes { get; set; }

    }
}
