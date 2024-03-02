using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Administration
{
    public class Machine : BaseObject, IObjectWithValidation
    {
        private string id;
        [PrimaryKey]
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string fullName;

        [UpdateWatch]
        public string FullName
        {
            get { return fullName; }
            set
            {
                fullName = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        [UpdateWatch]
        public string SerialNumber { get; set; }

        [UpdateWatch]
        public string Make { get; set; }

        [UpdateWatch]
        public string Model { get; set; }

        [UpdateWatch]
        public bool OutOfService { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Id));
            ValidateProperty(nameof(FullName));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Id))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Id))
                {
                    AddError(propertyName, "Id must not be empty");
                    return;
                }
                if (Id.Length > 3)
                {
                    AddError(propertyName, "Id should not be longer than three characters");
                }

                if (!ValidationHelper.StringIsUpperCaseAndNumbers(Id))
                {
                    AddError(propertyName, "Id should consist of upper case alphanumeric characters");
                }

                return;
            }
            else if (propertyName == nameof(FullName))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(FullName))
                {
                    AddError(propertyName, "Full Name must not be empty");
                    return;
                }

                if (FullName.Trim() != FullName)
                {
                    AddError(propertyName, "Full Name should not be contain leading or trailing whitespace");
                }

                if (!ValidationHelper.StringIsAlphanumeric(FullName, allowSpace: true))
                {
                    AddError(propertyName, "Full Name should contain only alphanumeric characters and space");
                }

                return;
            }
            else if (propertyName == nameof(SerialNumber))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(SerialNumber))
                {
                    AddError(propertyName, "Serial Number must not be empty");
                    return;
                }

                if (SerialNumber.Trim() != SerialNumber)
                {
                    AddError(propertyName, "Serial Number should not be contain leading or trailing whitespace");
                }


                return;
            }


            throw new Exception($"Validation for {propertyName} has not been configured.");
        }

        public virtual bool CanRun(ScheduleItem item)
        {
            return item is not LatheManufactureOrder;
        }
    }
}
