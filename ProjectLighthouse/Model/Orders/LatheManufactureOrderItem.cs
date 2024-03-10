using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Requests;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProjectLighthouse.Model.Orders
{
    public class LatheManufactureOrderItem : BaseObject, IObjectWithValidation, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public string AssignedMO { get; set; }
        [NotNull]
        public string ProductName { get; set; }
        public int ProductId { get; set; }

        private int requiredQuantity;
        [UpdateWatch]
        public int RequiredQuantity
        {
            get { return requiredQuantity; }
            set
            {
                requiredQuantity = value;
                ValidateProperty(nameof(TargetQuantity));
                OnPropertyChanged();
            }
        }


        private int targetQuantity;
        [NotNull]
        [UpdateWatch]
        public int TargetQuantity
        {
            get { return targetQuantity; }
            set
            {
                targetQuantity = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int quantityMade;
        [UpdateWatch]
        public int QuantityMade
        {
            get { return quantityMade; }
            set { quantityMade = value; OnPropertyChanged(); }
        }

        private int quantityReject;
        [UpdateWatch]
        public int QuantityReject
        {
            get { return quantityReject; }
            set { quantityReject = value; OnPropertyChanged(); }
        }

        private int quantityDelivered;
        [UpdateWatch]
        public int QuantityDelivered
        {
            get { return quantityDelivered; }
            set { quantityDelivered = value; OnPropertyChanged(); }
        }


        private int cycleTime;
        [UpdateWatch]
        public int CycleTime
        {
            get { return cycleTime; }
            set
            {
                cycleTime = value;
                OnPropertyChanged();
            }
        }

        private int? previousCycleTime;
        [UpdateWatch]
        public int? PreviousCycleTime
        {
            get { return previousCycleTime; }
            set
            {
                previousCycleTime = value;
                OnPropertyChanged();
            }
        }

        private int? modelledCycleTime;
        [UpdateWatch]
        public int? ModelledCycleTime
        {
            get { return modelledCycleTime; }
            set { modelledCycleTime = value; OnPropertyChanged(); }
        }


        private double majorLength;
        [UpdateWatch]
        public double MajorLength
        {
            get { return majorLength; }
            set
            {
                if (value == majorLength) return;
                majorLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private double partOffLength;
        [UpdateWatch]
        public double PartOffLength
        {
            get { return partOffLength; }
            set
            {
                if (value == partOffLength) return;
                partOffLength = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public double MajorDiameter { get; set; }

        public int DrawingId { get; set; }

        private DateTime dateRequired;
        [UpdateWatch]
        public DateTime DateRequired
        {
            get { return dateRequired; }
            set { dateRequired = value; OnPropertyChanged(); }
        }

        public DateTime DateAdded { get; set; }
        public string AddedBy { get; set; }
        public bool IsSpecialPart { get; set; }

        private string updatedBy;
        public string UpdatedBy
        {
            get { return updatedBy; }
            set { updatedBy = value; OnPropertyChanged(); }
        }

        private DateTime updatedAt;
        public DateTime UpdatedAt
        {
            get { return updatedAt; }
            set { updatedAt = value; OnPropertyChanged(); }
        }

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public int RecommendedQuantity { get; set; }

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public int QuantityInStock { get; set; }

        public LatheManufactureOrderItem()
        {

        }

        private void SetupFromProduct(TurnedProduct fromProduct)
        {
            ProductName = fromProduct.ProductName;
            ProductId = fromProduct.Id;
            PreviousCycleTime = fromProduct.CycleTime > 0 ? fromProduct.CycleTime : null;
            MajorDiameter = fromProduct.MajorDiameter;
            MajorLength = fromProduct.MajorLength;
            PartOffLength = fromProduct.PartOffLength;
            DateAdded = DateTime.Now;
            AddedBy = App.CurrentUser.GetFullName();
            IsSpecialPart = fromProduct.IsSpecialPart;

            RequiredQuantity = 0;
            TargetQuantity = Math.Max(fromProduct.GetRecommendedQuantity(), 1);

            QuantityInStock = fromProduct.QuantityInStock;
            RecommendedQuantity = fromProduct.GetRecommendedQuantity();
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct)
        {
            SetupFromProduct(fromProduct);
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity)
        {
            SetupFromProduct(fromProduct);

            RequiredQuantity = requiredQuantity;
            TargetQuantity = RequestsEngine.RoundQuantity(Math.Max(TargetQuantity, RequiredQuantity), roundUp: true);
        }

        public LatheManufactureOrderItem(TurnedProduct fromProduct, int requiredQuantity, DateTime dateRequired)
        {
            SetupFromProduct(fromProduct);

            DateRequired = dateRequired;
            RequiredQuantity = requiredQuantity;
            TargetQuantity = RequestsEngine.RoundQuantity(Math.Max(TargetQuantity, RequiredQuantity), roundUp: true);
        }

        public override string ToString()
        {
            return ProductName;
        }

        public int PlannedCycleTime()
        {
            return PreviousCycleTime ?? ModelledCycleTime ?? RequestsEngine.EstimateCycleTime(MajorDiameter);
        }

        public int GetTimeToMakeRequired()
        {
            return (CycleTime == 0 ? PlannedCycleTime() : CycleTime) * RequiredQuantity;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(RequiredQuantity));
            ValidateProperty(nameof(TargetQuantity));
            ValidateProperty(nameof(MajorLength));
            ValidateProperty(nameof(PartOffLength));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(TargetQuantity))
            {
                ClearErrors(propertyName);

                if (TargetQuantity <= 0)
                {
                    AddError(propertyName, "Target Quantity must be greater than zero");
                    return;
                }

                if (TargetQuantity < RequiredQuantity)
                {
                    AddError(propertyName, "Target Quantity must be greater than or equal to Required Quantity");
                    return;
                }

                if (TargetQuantity > 100_000)
                {
                    AddError(propertyName, "Max Target Quantity is 100,000");
                }

                return;
            }
            else if (propertyName == nameof(RequiredQuantity))
            {
                ClearErrors(propertyName);

                if (TargetQuantity < 0)
                {
                    AddError(propertyName, "Required Quantity must be greater than or equal to zero");
                    return;
                }

                if (RequiredQuantity > 100_000)
                {
                    AddError(propertyName, "Max Required Quantity is 100,000");
                }

                return;
            }
            else if (propertyName == nameof(MajorLength))
            {
                ClearErrors(propertyName);

                if (MajorLength <= 0)
                {
                    AddError(propertyName, "Major Length must be greater than zero");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(PartOffLength))
            {
                ClearErrors(propertyName);

                if (PartOffLength < 0)
                {
                    AddError(propertyName, "Extra Material must be greater than or equal to zero");
                    return;
                }

                return;
            }

            throw new NotImplementedException();
        }

        public virtual bool IsUpdated(LatheManufactureOrderItem otherItem)
        {
            try
            {
                return GetListOfChanges(otherItem).Count != 0;
            }
            catch
            {
                throw;
            }
        }

        public virtual List<string> GetListOfChanges(LatheManufactureOrderItem otherItem)
        {
            if (otherItem.Id != Id)
            {
                throw new InvalidOperationException($"Items being compared must have the same ID");
            }
            List<string> results = new();

            PropertyInfo[] properties = typeof(LatheManufactureOrderItem).GetProperties();

            bool mod = false;


            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(otherItem)))
                {
                    if (!mod)
                    {
                        results.Add($"{DateTime.Now:s} | {App.CurrentUser.UserName}");
                    }
                    StringBuilder sb = new();
                    sb.AppendLine($"\t{property.Name} modified");
                    sb.AppendLine($"\t\tfrom: '{property.GetValue(this) ?? "null"}'");
                    sb.AppendLine($"\t\tto  : '{property.GetValue(otherItem) ?? "null"}'");

                    results.Add(sb.ToString());

                    mod = true;
                }

            }

            return results;
        }
    }
}
