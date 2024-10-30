using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Administration
{
    public class Lathe : Machine, IObjectWithValidation
    {
        [UpdateWatch]
        public string IPAddress { get; set; }

        [UpdateWatch]
        public string ControllerReference { get; set; }

        private double maxDiameter;
        [UpdateWatch]
        public double MaxDiameter
        {
            get { return maxDiameter; }
            set
            {
                maxDiameter = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double softMinDiameter;
        [UpdateWatch]
        public double SoftMinDiameter
        {
            get { return softMinDiameter; }
            set
            {
                softMinDiameter = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double softMaxDiameter;
        [UpdateWatch]
        public double SoftMaxDiameter
        {
            get { return softMaxDiameter; }
            set
            {
                softMaxDiameter = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        private double maxLength;
        [UpdateWatch]
        public double MaxLength
        {
            get { return maxLength; }
            set
            {
                maxLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double partOff;

        [UpdateWatch]
        public double PartOff
        {
            get { return partOff; }
            set
            {
                partOff = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        [UpdateWatch]
        public string Remarks { get; set; }

        [Ignore]
        public List<MaintenanceEvent> Maintenance { get; set; }
        [Ignore]
        public List<Attachment> Attachments { get; set; }
        [Ignore]
        public List<Attachment> ServiceRecords { get; set; }

        private string? features;
        [UpdateWatch]
        public string? Features
        {
            get { return features; }
            set
            {
                features = value;
                OnPropertyChanged();
            }
        }


        [Ignore]
        public List<string> FeatureList
        {
            get
            {
                if (Features is null) return new();
                return Features.Split(";").ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    Features = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                Features = null;
                OnPropertyChanged();
            }
        }


        public string MachineKey
        {
            get
            {
                return $"{Make} {Model}";
            }
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Lathe>(serialised);
        }

        public override string ToString()
        {
            return FullName;
        }

        public new void ValidateAll()
        {
            base.ValidateAll();
            ValidateProperty(nameof(SoftMinDiameter));
            ValidateProperty(nameof(SoftMaxDiameter));
            ValidateProperty(nameof(MaxDiameter));
            ValidateProperty(nameof(MaxLength));
            ValidateProperty(nameof(PartOff));
        }

        public new void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(MaxDiameter))
            {
                ClearErrors(propertyName);

                if (MaxDiameter <= 0)
                {
                    AddError(propertyName, "Max Diameter must be greater than zero");
                    return;
                }

                if (MaxDiameter > 1000)
                {
                    AddError(propertyName, "Max Diameter must be less than or equal to 1,000mm");
                    return;
                }


                return;
            }
            else if (propertyName == nameof(SoftMaxDiameter))
            {
                ClearErrors(propertyName);

                if (SoftMaxDiameter <= 0)
                {
                    AddError(propertyName, "Soft Max Diameter must be greater than zero");
                    return;
                }

                if (SoftMaxDiameter > MaxDiameter)
                {
                    AddError(propertyName, "Soft Max Diameter must be less than or equal to Max Diameter");
                    return;
                }


                return;
            }
            else if (propertyName == nameof(SoftMinDiameter))
            {
                ClearErrors(propertyName);

                if (SoftMinDiameter < 0)
                {
                    AddError(propertyName, "Soft Max Diameter must be greater than or equal to zero");
                    return;
                }

                if (SoftMinDiameter >= MaxDiameter)
                {
                    AddError(propertyName, "Soft Min Diameter must be less than Max Diameter");
                    return;
                }

                if (SoftMinDiameter >= SoftMaxDiameter)
                {
                    AddError(propertyName, "Soft Min Diameter must be less than Soft Max Diameter");
                    return;
                }


                return;
            }
            else if (propertyName == nameof(MaxLength))
            {
                ClearErrors(propertyName);

                if (MaxLength <= 0 || MaxLength <= PartOff)
                {
                    AddError(propertyName, "Max part length must be greater than zero or the part-off length, whichever is larger");
                }

                if (MaxLength > 1500)
                {
                    AddError(propertyName, "Max part length must be less than 1,500mm");
                }

                return;
            }
            else if (propertyName == nameof(PartOff))
            {
                ClearErrors(propertyName);

                if (PartOff <= 0)
                {
                    AddError(propertyName, "Part off length must be greater than or equal to zero");
                }

                if (PartOff > 20)
                {
                    AddError(propertyName, "Part off length must be less than 20mm");
                }

                return;
            }

            throw new Exception($"Validation for {propertyName} has not been configured.");
        }

        //public List<string> GetChanges(Lathe otherLathe)
        //{
        //    PropertyInfo[] properties = typeof(Lathe).GetProperties();
        //    bool mod = false;
        //    List<string> changes = new();

        //    foreach (PropertyInfo property in properties)
        //    {
        //        bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;

        //        if (!watchPropForChanges)
        //        {
        //            continue;
        //        }

        //        if (!Equals(property.GetValue(this), property.GetValue(otherLathe)))
        //        {
        //            changes.Add($"{property.Name} changed from '{property.GetValue(this) ?? "null"}' to '{property.GetValue(otherLathe) ?? "null"}'");

        //            mod = true;
        //        }

        //    }

        //    if (mod)
        //    {
        //        //string path = App.ROOT_PATH + @"lib\logs\" + Id + ".log";

        //        //File.AppendAllText(path, sb.ToString());
        //    }

        //    return changes;
        //}

        //public bool IsUpdated(Lathe otherLathe)
        //{
        //    List<string> changes = GetChanges(otherLathe);

        //    return changes.Count > 0;
        //}

        public override bool CanRun(ScheduleItem item)
        {
            if (item is MachineService)
            {
                return true;
            }
            else if (item is LatheManufactureOrder order)
            {
                if (order.Bar.MajorDiameter > this.MaxDiameter)
                {
                    return false;
                }

                if (order.OrderItems.Any(x => x.MajorLength > this.MaxLength))
                {
                    return false;
                }

                if (order.RequiredFeaturesList.Any(x => !FeatureList.Contains(x)))
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
