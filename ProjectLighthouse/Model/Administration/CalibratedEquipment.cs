using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Administration
{
    public class CalibratedEquipment : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string EquipmentId { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Type { get; set; } = "";
        public string Location { get; set; } = "";

        public DateTime EnteredSystem { get; set; }
        public DateTime LastVisualCheck { get; set; }
        public DateTime LastCalibrated { get; set; }
        public DateTime NextDue
        {
            get { return LastCalibrated.AddMonths(CalibrationIntervalMonths); }
        }
        public int CalibrationIntervalMonths { get; set; }
        public bool UKAS { get; set; }
        public bool RequiresCalibration { get; set; }
        public bool RequiresInternalChecks { get; set; }
        public bool IsOutOfService { get; set; }
        public string AddedBy { get; set; }

        public string Notes { get; set; }

        public List<CalibrationCertificate> Certificates = new();

        public override string ToString()
        {
            return EquipmentId;
        }

        public bool CalibrationHasLapsed()
        {
            if (!RequiresCalibration || IsOutOfService)
            {
                return false;
            }

            return NextDue < DateTime.Now;
        }

        public object Clone()
        {
            return new CalibratedEquipment() 
            { 
                Id = Id,
                EquipmentId = EquipmentId,
                Make = Make,
                Model = Model,
                SerialNumber = SerialNumber,
                Type = Type,
                Location = Location,
                EnteredSystem = EnteredSystem,
                LastVisualCheck = LastVisualCheck,
                LastCalibrated = LastCalibrated,
                CalibrationIntervalMonths = CalibrationIntervalMonths,
                UKAS = UKAS,
                RequiresCalibration = RequiresCalibration,
                RequiresInternalChecks = RequiresInternalChecks,
                IsOutOfService = IsOutOfService,
                AddedBy = AddedBy,
                Notes = Notes,
            };

        }
    }
}
