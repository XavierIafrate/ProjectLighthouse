using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Requests
{
    public class RequestItem : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int ItemId { get; set; }

        [SQLite.Ignore]
        public TurnedProduct? Item { get; set; }

        public int QuantityRequired { get; set; }
        public DateTime? DateRequired { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public string EditedBy { get; set; }
        public DateTime EditedAt { get; set; }

        public int? OrderItemId { get; set; }

        public void ValidateAll()
        {
            ValidateProperty(nameof(DateRequired));
            ValidateProperty(nameof(QuantityRequired));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(DateRequired))
            {
                ClearErrors(propertyName);

                if (DateRequired is null)
                {
                    AddError(propertyName, "Required Date must be given");
                    return;
                }

                if (DateRequired < CreatedAt)
                {
                    AddError(propertyName, $"Date Required must be after request date");
                }
                return;
            }
            else if (propertyName == nameof(QuantityRequired))
            {
                ClearErrors(propertyName);

                if (QuantityRequired < 1)
                {
                    AddError(propertyName, "Required Quantity must be greater than or equal to 1");
                }

                if (QuantityRequired > 100_000)
                {
                    AddError(propertyName, "Required Quantity must be less than or equal to 100,000");
                }

                return;
            }


            throw new Exception($"Validation for {propertyName} has not been configured.");
        }
    }
}
