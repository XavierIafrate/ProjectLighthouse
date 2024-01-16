using SQLite;
using System;

namespace ProjectLighthouse.Model.Orders
{
    public class Lot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string Order { get; set; }
        public string AddedBy { get; set; }
        public int Quantity { get; set; }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                ExcelDate = $"{Date:dd/MM/yyyy HH:mm:ss}";
            }
        }

        public DateTime DateProduced { get; set; }
        public string ExcelDate { get; set; }
        public bool IsReject { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsDelivered { get; set; }
        public string MaterialBatch { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string FromMachine { get; set; }
        public string Remarks { get; set; }
        public bool AllowDelivery { get; set; }
        public int CycleTime { get; set; }

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public Action<Lot> RequestToEdit { get; set; }

        public void NotifyRequestToEdit()
        {
            RequestToEdit?.Invoke(this);
        }

        public object Clone()
        {
            return new Lot
            {
                ID = ID,
                ProductName = ProductName,
                Order = Order,
                AddedBy = AddedBy,
                Quantity = Quantity,
                Date = Date,
                DateProduced = DateProduced,
                ExcelDate = ExcelDate,
                IsReject = IsReject,
                IsAccepted = IsAccepted,
                IsDelivered = IsDelivered,
                AllowDelivery = AllowDelivery,
                MaterialBatch = MaterialBatch,
                ModifiedBy = ModifiedBy,
                ModifiedAt = ModifiedAt,
                FromMachine = FromMachine,
                Remarks = Remarks,
                CycleTime = CycleTime,
            };
        }
    }
}
