using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Quality
{
    public class CalibratedEquipment : BaseObject, IObjectWithValidation, ICloneable, IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string EquipmentId { get; set; }

        private string make = "";
        public string Make
        {
            get { return make; }
            set
            {
                make = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string model = "";
        public string Model
        {
            get { return model; }
            set
            {
                model = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string serialNumber = "";
        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                serialNumber = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string type = "";
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string location = "";
        public string Location
        {
            get { return location; }
            set
            {
                location = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int calibrationIntervalMonths;
        public int CalibrationIntervalMonths
        {
            get { return calibrationIntervalMonths; }
            set
            {
                calibrationIntervalMonths = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }



        public DateTime EnteredSystem { get; set; }
        public DateTime LastVisualCheck { get; set; }
        public DateTime LastCalibrated { get; set; }
        public DateTime NextDue
        {
            get { return LastCalibrated.AddMonths(CalibrationIntervalMonths); }
        }
        public bool UKAS { get; set; }
        public bool RequiresCalibration { get; set; }
        public bool RequiresInternalChecks { get; set; }
        public bool IsOutOfService { get; set; }
        public bool IsOutForCal { get; set; }
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
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CalibratedEquipment>(serialised);

        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Make));
            ValidateProperty(nameof(Model));
            ValidateProperty(nameof(SerialNumber));
            ValidateProperty(nameof(Type));
            ValidateProperty(nameof(Location));
            ValidateProperty(nameof(CalibrationIntervalMonths));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Make))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Make))
                {
                    AddError(propertyName, "Make cannot be empty");
                    return;
                }

                if (Make.Trim() != Make)
                {
                    AddError(propertyName, "Make cannot have leading or trailing whitespace");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(Model))
            {
                ClearErrors(propertyName);
                if (string.IsNullOrWhiteSpace(Model))
                {
                    AddError(propertyName, "Model cannot be empty");
                    return;
                }

                if (Model.Trim() != Model)
                {
                    AddError(propertyName, "Model cannot have leading or trailing whitespace");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(SerialNumber))
            {
                ClearErrors(propertyName);

                // not allowed whitespace only
                if (string.IsNullOrWhiteSpace(SerialNumber) && !string.IsNullOrEmpty(SerialNumber))
                {
                    AddError(propertyName, "Serial Number cannot be whitespace");
                    return;
                }

                if (SerialNumber.Trim() != SerialNumber)
                {
                    AddError(propertyName, "Serial Number cannot have leading or trailing whitespace");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(Type))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Type))
                {
                    AddError(propertyName, "Type cannot be empty");
                    return;
                }

                if (Type.Trim() != Type)
                {
                    AddError(propertyName, "Type cannot have leading or trailing whitespace");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(Location))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Location))
                {
                    AddError(propertyName, "Location cannot be empty");
                    return;
                }

                if (Location.Trim() != Location)
                {
                    AddError(propertyName, "Location cannot have leading or trailing whitespace");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(CalibrationIntervalMonths))
            {
                ClearErrors(propertyName);

                if (CalibrationIntervalMonths <= 0)
                {
                    AddError(propertyName, "Interval must be greater than zero");
                    return;
                }

                if (CalibrationIntervalMonths > 60)
                {
                    AddError(propertyName, "Interval must be less or equal to than 60");
                    return;
                }

                return;
            }


            throw new NotImplementedException();
        }
    }
}
