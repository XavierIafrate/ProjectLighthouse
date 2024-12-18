using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Orders
{
    public class MachineBreakdown : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string OrderName { get; set; }

        private string breakdownCode = string.Empty;
        public string BreakdownCode
        {
            get { return breakdownCode; }
            set
            {
                value ??= string.Empty;
                breakdownCode = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private DateTime breakdownStarted;
        public DateTime BreakdownStarted
        {
            get { return breakdownStarted; }
            set
            {
                breakdownStarted = value;
                ValidateProperty();
                ValidateProperty(nameof(BreakdownEnded));
                OnPropertyChanged();
            }
        }

        private DateTime breakdownEnded;
        public DateTime BreakdownEnded
        {
            get { return breakdownEnded; }
            set
            {
                breakdownEnded = value;
                ValidateProperty();
                ValidateProperty(nameof(BreakdownStarted));
                OnPropertyChanged();
            }
        }


        private string comment = string.Empty;
        public string Comment
        {
            get { return comment; }
            set
            {
                value ??= string.Empty;
                comment = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        [Ignore]
        public BreakdownCode BreakdownMeta { get; set; }
        [Ignore]
        public int TimeElapsed => (int)(BreakdownEnded - BreakdownStarted).TotalSeconds;

        public void ValidateAll()
        {
            ValidateProperty(nameof(BreakdownCode));
            ValidateProperty(nameof(Comment));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(BreakdownCode))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(BreakdownCode))
                {
                    AddError(nameof(BreakdownCode), "Breakdown code must not be empty");
                }

                return;
            }
            else if (propertyName == nameof(Comment))
            {
                ClearErrors(propertyName);

                if (Comment.Length > 150)
                {
                    AddError(nameof(Comment), "Comment length must be less than or equal to 150 characters");
                }

                return;
            }
            else if (propertyName == nameof(BreakdownStarted))
            {
                ClearErrors(propertyName);

                if (BreakdownStarted >= BreakdownEnded)
                {
                    AddError(nameof(BreakdownStarted), "Breakdown must start before it ends");
                }

                return;
            }
            else if (propertyName == nameof(BreakdownEnded))
            {
                ClearErrors(propertyName);

                if (BreakdownEnded < BreakdownStarted)
                {
                    AddError(nameof(BreakdownEnded), "Breakdown must end before it starts");
                }

                return;
            }

            throw new NotImplementedException();
        }

        internal bool ValidateOverlap(List<MachineBreakdown> breakdowns)
        {
            List<MachineBreakdown> sortedBreakdowns = breakdowns.OrderBy(x => x.BreakdownStarted).ToList();

            foreach (MachineBreakdown breakdown in sortedBreakdowns)
            {
                DateTime start = breakdown.BreakdownStarted;
                DateTime end = breakdown.BreakdownEnded;

                if (BreakdownStarted >= start && BreakdownEnded <= end)
                {
                    // other record fully overlaps
                    return false;
                }

                if (BreakdownStarted <= start && BreakdownEnded > end)
                {
                    // this record fully overlaps
                    return false;
                }

                if ((BreakdownEnded > start && BreakdownEnded <= end) || (BreakdownStarted >= start && BreakdownStarted < end))
                {
                    // partial overlap
                    return false;
                }

            }

            return true;
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MachineBreakdown>(serialised);
        }

        internal bool IsUpdated(MachineBreakdown otherBreakdown)
        {
            if (otherBreakdown.Id != Id)
            {
                throw new InvalidOperationException($"Cannot compare Breakdown {Id} with record {otherBreakdown.Id}");
            }

            if (BreakdownStarted != otherBreakdown.BreakdownStarted)
            {
                return true;
            }

            if (BreakdownEnded != otherBreakdown.BreakdownEnded)
            {
                return true;
            }

            if (Comment != otherBreakdown.Comment)
            {
                return true;
            }

            if (BreakdownCode != otherBreakdown.BreakdownCode)
            {
                return true;
            }

            return false;
        }
    }
}
