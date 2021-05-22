using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    class Lot
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string ExcelDate { get; set; }
        public bool IsReject { get; set; }

        public void SetExcelDateTime()
        {
            ExcelDate = string.Format("{0:dd/MM/yyyy HH:mm:ss}", Date);
        }
    }
}
