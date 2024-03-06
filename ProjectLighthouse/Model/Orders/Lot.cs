using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Orders
{
    public class Lot : BaseObject, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        private string productName = string.Empty;
        public string ProductName
        {
            get
            {
                return productName;
            }
            set
            {
                productName = value;
                OnPropertyChanged();
            }
        }

        private string order = string.Empty;
        public string Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public string AddedBy { get; set; }

        private int quantity;
        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date { get; set; }

        private DateTime dateProduced;
        public DateTime DateProduced
        {
            get
            {
                return dateProduced;
            }
            set
            {
                dateProduced = value;
                OnPropertyChanged();
            }
        }

        private bool isReject;
        public bool IsReject
        {
            get
            {
                return isReject;
            }
            set
            {
                isReject = value;
                if (value)
                {
                    isAccepted = !value;
                    OnPropertyChanged(nameof(IsAccepted));
                }

                OnPropertyChanged();
            }
        }

        private bool isAccepted;
        public bool IsAccepted
        {
            get
            {
                return isAccepted;
            }
            set
            {
                isAccepted = value;
                if (value)
                {
                    isReject = !value;
                    OnPropertyChanged(nameof(IsReject));
                }
                OnPropertyChanged();
            }
        }

        private bool isDelivered;
        public bool IsDelivered
        {
            get
            {
                return isDelivered;
            }
            set
            {
                isDelivered = value;
                OnPropertyChanged();
            }
        }

        private string materialBatch = string.Empty;
        public string MaterialBatch
        {
            get
            {
                return materialBatch;
            }
            set
            {
                materialBatch = value.Trim();
                OnPropertyChanged();
            }
        }

        private string modifiedBy = string.Empty;
        public string ModifiedBy
        {
            get
            {
                return modifiedBy;
            }
            set
            {
                modifiedBy = value;
                OnPropertyChanged();
            }
        }

        private DateTime modifiedAt;
        public DateTime ModifiedAt
        {
            get
            {
                return modifiedAt;
            }
            set
            {
                modifiedAt = value;
                OnPropertyChanged();
            }
        }

        private string fromMachine = string.Empty;
        public string FromMachine
        {
            get
            {
                return fromMachine;
            }
            set
            {
                fromMachine = value;
                OnPropertyChanged();
            }
        }

        private string remarks = string.Empty;
        public string Remarks
        {
            get
            {
                return remarks;
            }
            set
            {
                remarks = value.Trim();
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private bool allowDelivery;
        public bool AllowDelivery
        {
            get
            {
                return allowDelivery;
            }
            set
            {
                allowDelivery = value;
                OnPropertyChanged();
            }
        }

        private int cycleTime;
        public int CycleTime
        {
            get
            {
                return cycleTime;
            }
            set
            {
                cycleTime = value;
                OnPropertyChanged();
            }
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Lot>(serialised);
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(ProductName));
            ValidateProperty(nameof(Order));
            ValidateProperty(nameof(Quantity));
            ValidateProperty(nameof(MaterialBatch));
            ValidateProperty(nameof(CycleTime));
            ValidateProperty(nameof(Remarks));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(ProductName))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    AddError(propertyName, "Product Name must be given a value");
                    return;
                }
                return;
            }
            else if (propertyName == nameof(Order))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Order))
                {
                    AddError(propertyName, "Order must be given a value");
                    return;
                }
                return;
            }
            else if (propertyName == nameof(Quantity))
            {
                ClearErrors(propertyName);

                if (Quantity <= 0)
                {
                    AddError(propertyName, "Quantity must greater than zero");
                    return;
                }

                return;
            }
            else if (propertyName == nameof(MaterialBatch))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(MaterialBatch))
                {
                    AddError(propertyName, "Material Batch must be given a value");
                    return;
                }
                return;
            }
            else if (propertyName == nameof(CycleTime))
            {
                ClearErrors(propertyName);

                if (CycleTime <= 0)
                {
                    AddError(propertyName, "Cycle Time must be greater than zero");
                    return;
                }
                return;
            }
            else if (propertyName == nameof(Remarks))
            {
                ClearErrors(propertyName);

                if (!string.IsNullOrEmpty(Remarks))
                {
                    if (Remarks.Length > 512)
                    {
                        AddError(propertyName, $"Character count exceeded for Remarks ({Remarks.Length:#,##0}/512)");
                    }
                }
                return;
            }


            throw new NotImplementedException();
        }

        public bool IsUpdated(Lot otherLot)
        {
            if (otherLot.ID != ID)
            {
                throw new InvalidOperationException($"Cannot compare Lot {ID} with record {otherLot.ID}");
            }

            if (Quantity != otherLot.Quantity)
            {
                return true;
            }

            if (DateProduced != otherLot.DateProduced)
            {
                return true;
            }

            if (IsAccepted != otherLot.IsAccepted)
            {
                return true;
            }

            if (IsReject != otherLot.IsReject)
            {
                return true;
            }

            if (IsDelivered != otherLot.IsDelivered)
            {
                return true;
            }

            if (MaterialBatch != otherLot.MaterialBatch)
            {
                return true;
            }

            if (AllowDelivery != otherLot.AllowDelivery)
            {
                return true;
            }

            if (CycleTime != otherLot.CycleTime)
            {
                return true;
            }

            if (Remarks != otherLot.Remarks)
            {
                return true;
            }

            return false;
        }
    }
}
