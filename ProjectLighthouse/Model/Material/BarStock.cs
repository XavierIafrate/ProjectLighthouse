using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Material
{
    public class BarStock : BaseObject, IObjectWithValidation, ICloneable
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

        private string? erpId;

        [Unique]
        [Import("ERP ID")]
        public string? ErpId
        {
            get { return erpId; }
            set
            {
                erpId = value;
                OnPropertyChanged();
            }
        }


        private bool isSyncing;
        [Import("Is Syncing")]
        public bool IsSyncing
        {
            get { return isSyncing; }
            set
            {
                if (isSyncing == value) return;

                isSyncing = value;
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
                if (value == size) return;
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

        private bool isHexagon;

        public bool IsHexagon
        {
            get { return isHexagon; }
            set
            {
                isHexagon = value;
                OnPropertyChanged();
            }
        }

        private bool isDormant;
        public bool IsDormant
        {
            get { return isDormant; }
            set
            {
                isDormant = value;
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


        [Ignore]
        public MaterialInfo MaterialData { get; set; }
        public int MaterialId { get; set; }


        public double? ExpectedCost
        {
            get
            {
                if (MaterialData is null) return null;
                if (MaterialData.Cost is null) return null;

                return MaterialData.Cost * GetUnitMassOfBar();
            }
        }



        public double MajorDiameter
        {
            get { return IsHexagon ? AcrossFlatsToAcrossCorners(Size) : Size; }
        }

        private static double AcrossFlatsToAcrossCorners(double af)
        {
            return (2 / Math.Sqrt(3)) * af;
        }

        public double GetUnitMassOfBar()
        {
            double area;
            double size_m = Size / 1000;


            if (IsHexagon)
            {
                area = 0.5 * Math.Sqrt(3) * Math.Pow(size_m, 2);
            }
            else
            {
                area = Math.PI * Math.Pow(size_m / 2, 2);
            }

            double mass = area;
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
            if (propertyName == nameof(Id))
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
            else if (propertyName == nameof(Size))
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

        public object Clone()
        {
            return new BarStock() 
            {
                Id = this.Id,
                MaterialId = this.MaterialId,   
                Length = this.Length,
                Size = this.Size,
                Cost = this.Cost,
                OnOrder = this.OnOrder,
                SuggestedStock = this.SuggestedStock,
                IsHexagon = this.IsHexagon,
                IsDormant = this.IsDormant,
                RequiresFeatures = this.RequiresFeatures,
                ErpId = this.ErpId,
                IsSyncing = this.IsSyncing,
            };
        }
    }
}
