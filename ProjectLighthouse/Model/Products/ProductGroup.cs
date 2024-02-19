using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class ProductGroup : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string name;
        [NotNull, Unique]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public int? ProductId { get; set; }
        public double? MinBarSize { get; set; }

        private double majorDiameter;
        public double MajorDiameter
        {
            get { return majorDiameter; }
            set
            {
                if (majorDiameter == value) return;

                majorDiameter = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private bool usesHexagonBar;

        public bool UsesHexagonBar
        {
            get { return usesHexagonBar; }
            set
            {
                usesHexagonBar = value;
                OnPropertyChanged();
            }
        }

        private string? defaultTimeCode;

        public string? DefaultTimeCode
        {
            get { return defaultTimeCode; }
            set
            {
                defaultTimeCode = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string? requiresFeatures;
        public string? RequiresFeatures
        {
            get { return requiresFeatures; }
            set
            {
                requiresFeatures = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public List<string> RequiresFeaturesList
        {
            get
            {
                if (RequiresFeatures is null) return new();
                return RequiresFeatures.Split(";").OrderBy(x => x).ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    RequiresFeatures = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                RequiresFeatures = null;
                OnPropertyChanged();
            }
        }

        private GroupStatus status;
        public GroupStatus Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged(); }
        }

        public bool Dormant { get => Status == GroupStatus.Dormant; }
        public bool InDevelopment { get => Status == GroupStatus.InDevelopment; }
        public bool Active { get => Status == GroupStatus.Active; }

        public enum GroupStatus
        {
            Dormant = 0,
            InDevelopment = 100,
            Active = 200,
        }

        public override string ToString()
        {
            return Name;
        }

        public double GetRequiredBarSize()
        {
            if (MinBarSize is null)
            {
                return MajorDiameter;
            }

            return Math.Max(MajorDiameter, (double)MinBarSize);
        }

        public BarStock? GetRequiredBarStock(List<BarStock> bars, int materialId)
        {
            bars = bars.Where(x => x.MaterialId == materialId && x.MajorDiameter >= GetRequiredBarSize() && x.IsHexagon == UsesHexagonBar)
                        .OrderBy(x => x.Size)
                        .ToList();

            if (bars.Count == 0)
            {
                return null;
            }

            return bars.First();
        }

        public void ValidateAll()
        {
            if (Id == -1) // Unassigned
            {
                return;
            }

            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(MajorDiameter));
            ValidateProperty(nameof(DefaultTimeCode));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (Id == -1) // Unassigned
            {
                return;
            }

            if (propertyName == nameof(Name))
            {
                ClearErrors(propertyName);
                if (string.IsNullOrEmpty(Name))
                {
                    AddError(nameof(Name), "Group Name cannot be empty");
                    return;
                }

                if (!ValidationHelper.IsValidProductName(Name))
                {
                    AddError(nameof(Name), "Group Name contains a non-standard character");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(MajorDiameter))
            {
                ClearErrors(propertyName);
                if (MajorDiameter <= 0)
                {
                    AddError(nameof(MajorDiameter), "Major Diameter must be greater than zero");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(DefaultTimeCode))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrEmpty(DefaultTimeCode)) return;

                try
                {
                    TimeModel _ = new(DefaultTimeCode);
                }
                catch
                {
                    AddError(nameof(DefaultTimeCode), "Time Code could not be parsed");
                }

                return;
            }


            throw new NotImplementedException();

        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ProductGroup>(serialised);
        }
    }
}
