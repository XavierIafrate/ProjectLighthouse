using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class NonTurnedItem : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }

        private string? exportProductName;

        [Unique]
        [Import("Delivery Name")]
        public string? ExportProductName
        {
            get { return exportProductName; }
            set
            {
                exportProductName = value;
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

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged(); }
        }

        private int cycleTime;
        public int CycleTime
        {
            get { return cycleTime; }
            set { cycleTime = value; OnPropertyChanged(); }
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(CycleTime));
            ValidateProperty(nameof(Description));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(propertyName);
                if (string.IsNullOrWhiteSpace(Name))
                {
                    AddError(nameof(Name), "Name cannot be empty");
                    return;
                }

                if (!ValidationHelper.IsValidProductName(Name))
                {
                    string illegalChars = ValidationHelper.GetInvalidProductNameChars(Name);
                    AddError(propertyName, $"Name must be a valid product code (illegal character(s): {illegalChars})");
                    return;
                }

                if (Name.ToUpperInvariant() == "ORDER")
                {
                    AddError(nameof(Name), "Name cannot be 'Order'");
                    return;
                }
                
                return;
            }
            else if (propertyName == nameof(CycleTime))
            {
                ClearErrors(propertyName);
                if (CycleTime < 0)
                {
                    AddError(nameof(CycleTime), "Cycle Time must be greater than or equal to zero");
                }
                return;
            }
            else if (propertyName == nameof(Description))
            {
                ClearErrors(propertyName);
                if (string.IsNullOrWhiteSpace(Description))
                {
                    AddError(nameof(Description), "Description cannot be empty");
                    return;
                }


                if (Description.Length > 24)
                {
                    AddError(nameof(Description), "Max length of description is 24 characters.");
                    return;
                }

                return;
            }


            throw new System.NotImplementedException();
        }
    }
}
