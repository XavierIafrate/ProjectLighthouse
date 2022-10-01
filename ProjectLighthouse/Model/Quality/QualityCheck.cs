using SQLite;
using System;

namespace ProjectLighthouse.Model.Quality
{
    public class QualityCheck
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Product { get; set; }
        public DateTime RaisedAt { get; set; }
        public string RaisedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }
        public string CheckedBy { get; set; }
        public DateTime CheckedAt { get; set; }
        public DateTime RequiredBy { get; set; }
        public string SpecificationDocument { get; set; }
        public string SpecificationDetails { get; set; }

        [Ignore]
        public string Status
        {
            get
            {
                if (IsAccepted)
                {
                    return "Accepted";
                }
                else if (IsRejected)
                {
                    return "Rejected";
                }
                else
                {
                    return "Pending";
                }
            }
        }

    }
}
