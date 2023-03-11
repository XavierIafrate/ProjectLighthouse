using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
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

        public int? ProgramId { get; set; }

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
            bars = bars.Where(x => x.MaterialId == materialId && x.Size >= GetRequiredBarSize())
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


            throw new NotImplementedException();

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
