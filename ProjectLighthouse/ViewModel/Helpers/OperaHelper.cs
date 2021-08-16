using DbfDataReader;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class OperaHelper
    {
        private const string dbFile = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_cname.dbf";

        public static async Task UpdateStockLevelsAsync()
        {
            if (!File.Exists(dbFile))
            {
                MessageBox.Show("Cannot locate database.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();
            string[] lookup = new string[products.Count];

            for (int i = 0; i < products.Count; i++)
                lookup[i] = products[i].ProductName;

            await Task.Run(() => UpdateRecords(products, lookup));
        }

        private static void UpdateRecords(List<TurnedProduct> products, string[] nameLookup)
        {

            List<OperaFields> results = new();

            using (DbfTable dbfTable = new(dbFile, Encoding.UTF8))
            {
                DbfHeader header = dbfTable.Header;
                Debug.WriteLine($"{header.RecordCount} in Automotion CNAME table.");

                int iStockRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Where(n => n.ColumnName == "CN_REF").Single());
                int iSalesOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Where(n => n.ColumnName == "CN_SALEORD").Single());
                int iStockQuantity = dbfTable.Columns.IndexOf(dbfTable.Columns.Where(n => n.ColumnName == "CN_INSTOCK").Single());
                int iPurchaseOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Where(n => n.ColumnName == "CN_ONORDER").Single());
                int iSell = dbfTable.Columns.IndexOf(dbfTable.Columns.Where(n => n.ColumnName == "CN_SELL").Single());

                DbfRecord dbfRecord = new(dbfTable);

                while (dbfTable.Read(dbfRecord))
                {

                    if (dbfRecord.IsDeleted)
                        continue;

                    string name = dbfRecord.Values[iStockRef].ToString();
                    if (!nameLookup.Contains(name))
                        continue;

                    _ = int.TryParse(dbfRecord.Values[iSalesOrder].ToString(), out int SalesOrder);
                    _ = int.TryParse(dbfRecord.Values[iStockQuantity].ToString(), out int Stock);
                    _ = int.TryParse(dbfRecord.Values[iPurchaseOrder].ToString(), out int OnOrder);
                    _ = int.TryParse(dbfRecord.Values[iSell].ToString(), out int Sell);

                    SalesOrder /= 100;
                    Stock /= 100;
                    OnOrder /= 100;

                    OperaFields record = new()
                    {
                        StockReference = name,
                        QtyInStock = Stock,
                        QtyPurchaseOrder = OnOrder,
                        QtySalesOrder = SalesOrder,
                        SellPrice = Sell
                    };

                    results.Add(record);

                    //Debug.Write($"NAME:{name}, SALESORDER:{SalesOrder}pcs, STOCK:{Stock}pcs, ONORDER:{OnOrder}\n");
                }
            }

            foreach (OperaFields r in results)
            {
                TurnedProduct productRecord = products.Find(x => x.ProductName == r.StockReference);
                if (productRecord == null)
                    continue;

                productRecord.QuantityInStock = r.QtyInStock;
                productRecord.QuantityOnPO = r.QtyPurchaseOrder;
                productRecord.QuantityOnSO = r.QtySalesOrder;
                productRecord.SellPrice = r.SellPrice;

                DatabaseHelper.Update<TurnedProduct>(productRecord);
            }
        }

        protected class OperaFields
        {
            public string StockReference { get; set; }
            public int QtyInStock { get; set; }
            public int QtyPurchaseOrder { get; set; }
            public int QtySalesOrder { get; set; }
            public int SellPrice { get; set; }
        }
    }
}
