using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Material
{
    public class BarStock : BaseObject, IObjectWithValidation
    {
        private string id;

        [PrimaryKey]
        public string Id
        {
            get { return id; }
            set 
            { 
                id = value.ToUpperInvariant();
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int length;
        public int Length
        {
            get { return length; }
            set 
            { 
                length = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double size;
        public double Size
        {
            get { return size; }
            set 
            { 
                if(value == size) return;
                size = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public double InStock { get; set; }
        public double OnOrder { get; set; }
        public int Cost { get; set; }

        private int suggestedStock;
        public int SuggestedStock
        {
            get { return suggestedStock; }
            set { suggestedStock = value; }
        }


        [Ignore]
        public MaterialInfo MaterialData { get; set; }
        public int MaterialId { get; set; }

        public double GetUnitMassOfBar()
        {
            double mass = 3.14159 * Math.Pow(Size / 2000, 2);
            mass *= (double)Length / 1000;
            mass *= MaterialData.Density;
            return mass;
        }

        public override string ToString()
        {
            return Id;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Id));
            ValidateProperty(nameof(Length));
            ValidateProperty(nameof(Size));
            ValidateProperty(nameof(SuggestedStock));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if(propertyName== nameof(Id))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Id))
                {
                    AddError(propertyName, "Id must be given a value");
                    return;
                }

                if (!ValidationHelper.IsValidProductName(Id))
                {
                    string illegalChars = ValidationHelper.GetInvalidProductNameChars(Id);
                    AddError(propertyName, $"Id must be a valid product code (illegal character(s): {illegalChars})");
                }

                if (Id.Length > 16)
                {
                    AddError(propertyName, "Id must have 16 characters or less");
                }
                return;
            }
            else if (propertyName == nameof(Length))
            {
                ClearErrors(propertyName);

                // Bar calculations take off 300mm from length, need to allow for some usable length
                if (Length < 500)
                {
                    AddError(propertyName, "Bar Length cannot be less than 500mm");
                }

                // Arbitrary limit that seems unlikely to increase (famous last words?)
                if (Length > 3000)
                {
                    AddError(propertyName, "Bar Length cannot be greater than 3000mm");
                }

                return;
            }
            else if (propertyName== nameof(Size))
            {
                ClearErrors(propertyName);

                // Limits are arbitrary
                if (Size < 3)
                {
                    AddError(propertyName, "Bar Size cannot be less than 3mm");
                }

                if (Size > 1000)
                {
                    AddError(propertyName, "Bar Size cannot be greater than 1000mm");
                }

                return;
            }
            else if (propertyName == nameof(SuggestedStock))
            {
                ClearErrors(propertyName);

                if (SuggestedStock < 0)
                {
                    AddError(propertyName, "Suggested stock must be greater than or equal to zero");
                }

                // I'd be impressed if it was more than this!
                if (SuggestedStock > 100_000)
                {
                    AddError(propertyName, "Max suggested stock is 100,000");
                }

                return;
            }



            throw new Exception($"Validation for {propertyName} has not been configured.");
        }
    }

}
