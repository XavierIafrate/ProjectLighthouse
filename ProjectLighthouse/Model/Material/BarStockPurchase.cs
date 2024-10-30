using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLighthouse.Model.Material
{
    public class BarStockPurchase
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Int64 OperaId { get; set; }

        public string PurchaseOrder { get; set; } 
        public DateTime DateRequired { get; set; } 

        public string BarId { get; set; }
        public int QuantityRequired { get; set; }
        public int QuantityReceived { get; set; }
        public double LineValue { get; set; }
        public DateTime? QuotedDate { get; set; }
        public bool IsQuoted { get; set; }
        public string SupplierAccount { get; set; }

        internal bool IsUpdated(BarStockPurchase newData)
        {
            if(OperaId != newData.OperaId)
            {
                throw new ArgumentException("Opera IDs must match.");
            }

            return
                   PurchaseOrder != newData.PurchaseOrder
                || DateRequired != newData.DateRequired
                || BarId != newData.BarId
                || QuantityRequired != newData.QuantityRequired
                || QuantityReceived != newData.QuantityReceived
                || LineValue != newData.LineValue
                || QuotedDate != newData.QuotedDate
                || IsQuoted != newData.IsQuoted
                || SupplierAccount != newData.SupplierAccount;
        }
    }
}
