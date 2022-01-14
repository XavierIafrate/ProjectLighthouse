using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class Lot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductName { get; set; }
        public string Order { get; set; }
        public string AddedBy { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
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

        [Ignore, CsvHelper.Configuration.Attributes.Ignore]
        public Action<Lot> RequestToEdit { get; set; }

        public void NotifyRequestToEdit()
        {
            RequestToEdit?.Invoke(this);
        }


        public void SetExcelDateTime()
        {
            ExcelDate = $"{Date:dd/MM/yyyy HH:mm:ss}";
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
                MaterialBatch = MaterialBatch,
                ModifiedBy = ModifiedBy,
                ModifiedAt = ModifiedAt,
                FromMachine = FromMachine,
                Remarks = Remarks
            };
        }
    }
}
