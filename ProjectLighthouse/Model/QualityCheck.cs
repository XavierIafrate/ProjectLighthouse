using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class QualityCheck
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Product { get; set; }
        public DateTime RaisedAt { get; set; }
        public string RaisedBy { get; set; }
        public bool Complete { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool Authorised { get; set; }
        public string AuthorisedBy { get; set; }
        public DateTime AuthorisedAt { get; set; }
        public DateTime RequiredBy { get; set; }
        public string SpecificationDocument { get; set; }
        public string SpecificationDetails { get; set; }

        public string Result { get; set;}
    }
}
