using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class TechnicalDrawingGroup
    {
        public string Name { get; set; }
        public DateTime? LastIssue { get; set; }
        public int CurrentRevision { get; set; }
        public TechnicalDrawing.Amendment Amendment { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        public bool AllDrawingsWithdrawn { get; set; }
        public bool IsArchetypeGroup { get; set; }
    }
}
