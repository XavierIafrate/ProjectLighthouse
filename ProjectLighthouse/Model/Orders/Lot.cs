using ProjectLighthouse.Model.Core;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Orders
{
    public class Lot : BaseObject
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        private string productName;
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

        private string order;
        public string Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value; 
                OnPropertyChanged();
            }
        }

        public string AddedBy { get; set; }

        private int quantity;
        public int Quantity { 
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

        private string materialBatch;
        public string MaterialBatch
        {
            get
            {
                return materialBatch;
            }
            set
            {
                materialBatch = value;
                OnPropertyChanged();
            }
        }

        private string modifiedBy;
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

        private string fromMachine;
        public string FromMachine
        {
            get
            {
                return fromMachine ;
            }
            set
            {
                fromMachine = value;
                OnPropertyChanged();
            }
        }

        private string remarks;
        public string Remarks
        {
            get
            {
                return remarks;
            }
            set
            {
                remarks = value;
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
    }
}
