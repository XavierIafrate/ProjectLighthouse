﻿using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
#if DEBUG
using System.Diagnostics;
#endif

namespace ProjectLighthouse.Model.Orders
{
    public class GeneralManufactureOrder : ScheduleItem
    {
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        [UpdateWatch]
        public string POReference { get; set; }


        [UpdateWatch]
        public OrderState State { get; set; } = OrderState.Problem;

        public int NonTurnedItemId { get; set; }

        [UpdateWatch]
        public int RequiredQuantity { get; set; }
        [UpdateWatch]
        public int FinishedQuantity { get; set; }
        [UpdateWatch]
        public int DeliveredQuantity { get; set; }
        [UpdateWatch]
        public DateTime? RequiredDate { get; set; }

        [Ignore]
        public NonTurnedItem Item { get; set; }

        [Ignore]
        public List<Note> Notes { get; set; } = new();

        public bool IsUpdated(GeneralManufactureOrder OtherOrder)
        {
            if (OtherOrder.Name != Name)
            {
                throw new InvalidOperationException($"Cannot compare order {Name} with record {OtherOrder.Name}");
            }

            PropertyInfo[] properties = typeof(GeneralManufactureOrder).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(OtherOrder)))
                {
#if DEBUG
                    Debug.WriteLine($"'{property.Name}' has been modified:");
                    Debug.WriteLine($"   After: '{property.GetValue(this)}'");
                    Debug.WriteLine($"  Before: '{property.GetValue(OtherOrder)}'");
#endif
                    return true;
                }

            }

            return false;
        }
    }
}
