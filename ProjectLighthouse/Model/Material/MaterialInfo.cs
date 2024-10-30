using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Material
{
    public class MaterialInfo : BaseObject, IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Unique]
        public string MaterialCode { get; set; }

        [NotNull]
        public string MaterialText { get; set; }

        [NotNull]
        public string GradeText { get; set; }

        public double Density { get; set; }
        public int? Cost { get; set; }

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

        public override string ToString()
        {
            return $"{MaterialText}, Grade {GradeText}";
        }

        public double GetRate()
        {
            double cost = Convert.ToDouble(Cost);
            return cost/100;
        }
    }
}
