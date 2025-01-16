using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Material
{
    public class MaterialInfo : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation, ICloneable
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
            double cost = Convert.ToDouble(Cost ?? 0);
            return cost / 100;
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MaterialInfo>(serialised);
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(MaterialCode));
            ValidateProperty(nameof(MaterialText));
            ValidateProperty(nameof(GradeText));
            ValidateProperty(nameof(Density));
            ValidateProperty(nameof(Cost));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(MaterialCode))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(MaterialCode))
                {
                    AddError(propertyName, "Material Code cannot be empty");
                    return;
                }

                if (MaterialCode.Length > 5)
                {
                    AddError(propertyName, "Material Code cannot be longer than 5 characters");
                }

                if (MaterialCode.Length <= 1)
                {
                    AddError(propertyName, "Material Code must be longer than 1 character");
                }

                if (!ValidationHelper.IsValidProductName(MaterialCode))
                {
                    string invalidChars = ValidationHelper.GetInvalidProductNameChars(MaterialCode);
                    AddError(propertyName, $"Material Code must be a valid product code (invalid characters: {invalidChars})");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(MaterialText))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(MaterialText))
                {
                    AddError(propertyName, "Material Description cannot be empty");
                    return;
                }

                if (MaterialText.Trim() != MaterialText)
                {
                    AddError(propertyName, "Material Description has leading or trailing whitespace");
                    return;
                }

                if (MaterialText.Length > 20)
                {
                    AddError(propertyName, $"Material Description has a max length of 20 characters (currently {MaterialText.Length})");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(GradeText))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(GradeText))
                {
                    AddError(propertyName, "Grade cannot be empty");
                    return;
                }

                if (GradeText.Trim() != GradeText)
                {
                    AddError(propertyName, "Grade has leading or trailing whitespace");
                    return;
                }

                if (GradeText.Length > 15)
                {
                    AddError(propertyName, $"Grade has a max length of 15 characters (currently {GradeText.Length})");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(Density))
            {
                ClearErrors(propertyName);

                if(Density < 0)
                {
                    AddError(propertyName, "Density must be greater than or equal to zero");
                }

                if(Density > 22_600)
                {
                    AddError(propertyName, "Density must be less than or equal to 22,600. The densest material on Earth is Osmium at 22,600kg/m3.");
                }

                return;
            }
            else if (propertyName == nameof(Cost))
            {
                ClearErrors(propertyName);

                if(Cost is null)
                {
                    return;
                }

                if(Cost < 0)
                {
                    AddError(propertyName, "Cost must be greater than or equal to zero");
                }

                if(Cost >= 1_000_000)
                {
                    AddError(propertyName, "Cost must be less than 1,000,000");
                }

                return;
            }

            throw new NotImplementedException();
        }
    }
}
