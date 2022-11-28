using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Research;
using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Research
{
    public class ResearchProject : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ProjectCode { get { return $"D{Id:00000}"; } }
        public string ProjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public int? Owner { get; set; }

        public string RootDirectory { get; set; }

        [Ignore]
        public Uri Uri { get
            {
                return RootDirectory is null ? null : new(RootDirectory);
            }
        }

        public ResearchStage Stage { get; set; }

        [Ignore]
        public List<Note> Notes { get; set; }
        [Ignore]
        public List<ResearchArchetype> Archetypes { get; set; }
        [Ignore]
        public List<Attachment> Attachments { get; set; }
        [Ignore]
        public List<ResearchPurchase> Purchases { get; set; }
    }
}
