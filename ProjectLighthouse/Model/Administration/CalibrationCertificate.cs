using SQLite;
using System;

namespace ProjectLighthouse.Model.Administration
{
    public class CalibrationCertificate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Instrument { get; set; }
        public bool UKAS { get; set; }
        public string CalibrationHouse { get; set; }
        public string CertificateNumber { get; set; }
        public DateTime DateIssued { get; set; }
    }
}
