using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Xaml.Behaviors;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProjectLighthouse.Model.Administration
{
    public class Lathe : Machine, IObjectWithValidation, ICloneable
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

        public object Clone()
        {
            return this.MemberwiseClone() as Lathe;
        }

        public override string ToString()
        {
            return FullName;
        }

        public new void ValidateAll()
        {
            base.ValidateAll();
            ValidateProperty(nameof(MaxDiameter));
            ValidateProperty(nameof(MaxLength));
            ValidateProperty(nameof(PartOff));
        }

        public new void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(MaxDiameter))
            {
                ClearErrors(propertyName);

                if (MaxDiameter<=0)
                {
                    AddError(propertyName, "Max Diameter must be greater than or equal to zero");
                    return;
                }

                if (MaxDiameter > 1000)
                {
                    AddError(propertyName, "Max Diameter must be less than or equal to 1,000mm");
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

        public bool IsUpdated(Lathe otherLathe)
        {

            PropertyInfo[] properties = typeof(Lathe).GetProperties();
            bool mod = false;
            StringBuilder sb = new();

            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;

                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(otherLathe)))
                {
                    if (!mod)
                    {
                        sb.AppendLine($"{DateTime.Now:s} | {App.CurrentUser.UserName}");
                    }
                    sb.AppendLine($"\t{property.Name} modified");
                    sb.AppendLine($"\t\tfrom: '{property.GetValue(this) ?? "null"}'");
                    sb.AppendLine($"\t\tto  : '{property.GetValue(otherLathe) ?? "null"}'");

                    mod = true;
                }

            }

            if (mod)
            {
                string path = App.ROOT_PATH + @"lib\logs\" + Id + ".log";

                //File.AppendAllText(path, sb.ToString());
            }

            return mod;
        }


    }
}
