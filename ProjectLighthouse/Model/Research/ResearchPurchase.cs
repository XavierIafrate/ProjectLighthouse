using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Research
{
    public class ResearchPurchase : BaseObject, IAutoIncrementPrimaryKey, INotifyDataErrorInfo, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int ProjectId { get; set; }
        public int? ArchetypeId { get; set; }

        private string name = "";
        public string Name
        {
            get { return name; }
            set
            {
                name = value.Trim();
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string vendor = "";
        public string Vendor
        {
            get { return vendor; }
            set { vendor = value.Trim(); ValidateProperty(); OnPropertyChanged(); }
        }

        private string vendorCode = "";

        public string VendorCode
        {
            get { return vendorCode; }
            set { vendorCode = value.Trim(); ValidateProperty(); OnPropertyChanged(); }
        }

        public bool Authorised { get; set; } = false;
        public bool Ordered { get; set; } = false;

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int packSize = 1;

        public int PackSize
        {
            get { return packSize; }
            set { packSize = value; ValidateProperty(); OnPropertyChanged(); }
        }


        public string? Url { get; set; }



        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(nameof(Name));
                if (string.IsNullOrWhiteSpace(Name))
                {
                    AddError(nameof(Name), "Name cannot be empty");
                    return;
                }

                if(!ValidationHelper.StringIsAlphanumeric(Name, allowSpace:true))
                {
                    AddError(nameof(Name), "Name must consist of alphanumeric characters or space");
                }

            }
            else if (propertyName == nameof(Value))
            {
                ClearErrors(nameof(Value));
                if (Value < 0)
                {
                    AddError(nameof(Value), "Value must be greater than or equal to zero");
                }
            }
            else if (propertyName == nameof(Vendor))
            {
                ClearErrors(nameof(Vendor));
                if (string.IsNullOrWhiteSpace(Vendor))
                {
                    AddError(nameof(Vendor), "Vendor cannot be empty");
                }
            }
            else if (propertyName == nameof(VendorCode))
            {
                ClearErrors(nameof(VendorCode));
                if (string.IsNullOrWhiteSpace(VendorCode))
                {
                    AddError(nameof(VendorCode), "Vendor Code cannot be empty");
                    return;
                }

                if (!ValidationHelper.StringIsAlphanumeric(VendorCode, allowSpace: true))
                {
                    AddError(nameof(VendorCode), "Vendor Code must consist of alphanumeric characters or space");
                }
            }
            else if (propertyName == nameof(PackSize))
            {
                ClearErrors(nameof(PackSize));
                if (PackSize <= 0)
                {
                    AddError(nameof(PackSize), "Value must be greater than zero");
                }
            }
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(Value));
            ValidateProperty(nameof(Vendor));
            ValidateProperty(nameof(VendorCode));
            ValidateProperty(nameof(PackSize));
        }
    }
}
