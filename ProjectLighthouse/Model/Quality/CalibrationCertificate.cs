using SQLite;
using System;

namespace ProjectLighthouse.Model.Quality
{
    public class CalibrationCertificate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Instrument { get; set; }
        public bool UKAS { get; set; }
        public string CalibrationHouse { get; set; } = "";
        public string CertificateNumber { get; set; } = "";
        public bool IsPass { get; set; }
        public DateTime DateIssued { get; set; }
        public string Url { get; set; } = "";
        public string AddedBy { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
