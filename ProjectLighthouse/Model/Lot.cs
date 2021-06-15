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
        public string ExcelDate { get; set; }
        public bool IsReject { get; set; }
        public bool IsDelivered { get; set; }
        public string MaterialBatch { get; set; }

        public void SetExcelDateTime()
        {
            ExcelDate = string.Format("{0:dd/MM/yyyy HH:mm:ss}", Date);
        }
    }
}
