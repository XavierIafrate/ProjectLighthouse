using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Administration
{
    public class CalibratedEquipment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name
        {
            get { return $"CE{Id:0}"; }
        }

        public string Make { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }

        public DateTime EnteredSystem { get; set; }
        public DateTime LastVisualCheck { get; set; }
        public DateTime LastCalibrated { get; set; }
        public DateTime NextDue
        {
            get { return LastCalibrated.AddMonths(CalibrationIntervalMonths); }
        }
        public int CalibrationIntervalMonths { get; set; }
        public string CalibrationHouse { get; set; }
        public bool UKAS { get; set; }
        public CalibrationStatus Status { get; set; }

        public string Notes { get; set; }

        public enum CalibrationStatus { Indication, Regular, Failed } // or two bools?

        public List<CalibrationCertificate> Certificates = new();

        public event Action RequestToEdit;

        public void Edit()
        {
            RequestToEdit?.Invoke();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
