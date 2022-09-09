using DbfDataReader;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class OperaHelper
    {
        public static readonly string dbFile = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_cname.dbf";
        public static readonly string purchaseLinesTable = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_doline.dbf";
        public static readonly string purchaseHeaderTable = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_dohead.dbf";

        public static List<string> VerifyDeliveryNote(List<DeliveryItem> items)
        {
            List<string> errs = new();

            string[] relevantPurchases = items.Select(x => x.PurchaseOrderReference.ToUpper()).Distinct().ToArray();

            List<PurchaseLine> lines = GetPurchaseLines(relevantPurchases);
            //List<PurchaseHeader> headers = GetPurchaseHeaders(relevantPurchases);

            foreach (string order in relevantPurchases)
            {
                List<DeliveryItem> itemsOnPO = items.Where(x => x.PurchaseOrderReference == order).ToList();
                string[] uniqueItems = itemsOnPO.Select(x => x.Product).Distinct().ToArray();

                foreach (string item in uniqueItems)
                {
                    List<DeliveryItem> targets = itemsOnPO.Where(x => x.Product == item).ToList();
                    int qtyThisDel = targets.Sum(x => x.QuantityThisDelivery);
                    errs.AddRange(VerifyData(item, qtyThisDel, order, lines));
                }
            }

            return errs;
        }

        private static List<string> VerifyData(string item, int qty, string order, List<PurchaseLine> lines)
        {
            List<string> errs = new();

            List<PurchaseLine> line = lines.Where(x => x.Product == item && x.PurchaseReference == order).ToList();
            if (line.Count == 0)
            {
                errs.Add($"No lines found for {order}.");
                return errs;
            }

            if ((line.Sum(x => x.RequiredQuantity) - line.Sum(x => x.ReceivedQuantity)) < qty)
            {
                string msg = $"{item}: Delivery for {qty:#,##0}pcs will not succeed." +
                    $"{Environment.NewLine}\t{line.First().PurchaseReference}:" +
                    $"{Environment.NewLine}\t\t{line.Sum(x => x.RequiredQuantity):#,##0}pcs Listed" +
                    $"{Environment.NewLine}\t\t{line.Sum(x => x.ReceivedQuantity):#,##0}pcs Received";
                Debug.WriteLine(msg);

                errs.Add(msg);
            }

            return errs;
        }

        private static List<PurchaseLine> GetPurchaseLines(string[] refs)
        {
            List<PurchaseLine> results = new();

            using DbfTable dbfTable = new(purchaseLinesTable, Encoding.UTF8);
            DbfHeader tableHeader = dbfTable.Header;
            Console.WriteLine($"{tableHeader.RecordCount:#,##0} records in Automotion Purchase Order Lines table.");

            int iRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_DCREF"));
            int iAccount = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_ACCOUNT"));
            int iPartNumber = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_CNREF"));
            int iRequired = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_REQQTY"));
            int iReceived = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_RECQTY"));

            DbfRecord dbfRecord = new(dbfTable);

            int total_records = (int)tableHeader.RecordCount;
            int i = 0;

            while (dbfTable.Read(dbfRecord))
            {
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;

                Console.Write($"\rReading Opera... [  {percent_progress:#.00}%  ]");

                i++;

                if (dbfRecord.IsDeleted)
                {
                    continue;
                }

                string accountRef = dbfRecord.Values[iAccount].ToString();
                string poRef = dbfRecord.Values[iRef].ToString();
                if (accountRef != "AUTO01" || !refs.Contains(poRef.ToUpper()))
                {
                    continue;
                }

                string partNum = dbfRecord.Values[iPartNumber].ToString();

                _ = int.TryParse(dbfRecord.Values[iRequired].ToString(), out int required);
                _ = int.TryParse(dbfRecord.Values[iReceived].ToString(), out int received);

                required /= 100;
                received /= 100;

                PurchaseLine record = new()
                {

                    PurchaseReference = poRef,
                    Product = partNum,
                    Account = accountRef,
                    RequiredQuantity = required,
                    ReceivedQuantity = received,
                };

                results.Add(record);
            }

            return results;
        }

        // int uploads = await UploadPicturesAsync(GenerateTestImages(),
        // new Progress<int>(percent => progressBar1.Value = percent));

        public static void UpdateRecords(List<TurnedProduct> products, List<BarStock> barstock, string[] nameLookup)
        {
            int total_records;
            int i;

            List<OperaFields> results = new();
            List<OperaFields> bar_records = new();

            using (DbfTable dbfTable = new(dbFile, Encoding.UTF8))
            {
                DbfHeader header = dbfTable.Header;
                Console.WriteLine($"{header.RecordCount:#,##0} records in Automotion CNAME table.");

                int iStockRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_REF"));
                int iSearchRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_CAT"));
                int iSalesOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SALEORD"));
                int iStockQuantity = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_INSTOCK"));
                int iPurchaseOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_ONORDER"));
                int iSell = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SELL"));
                int iCost = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_COST"));

                DbfRecord dbfRecord = new(dbfTable);

                total_records = (int)header.RecordCount;
                i = 0;

                while (dbfTable.Read(dbfRecord))
                {
                    double percent_progress = (double)i / (double)total_records;
                    percent_progress *= 100;

                    Console.Write($"\rReading Opera... [  {percent_progress:#.00}%  ]");

                    i++;

                    if (dbfRecord.IsDeleted)
                        continue;

                    string name = dbfRecord.Values[iStockRef].ToString();
                    string searchref = dbfRecord.Values[iSearchRef].ToString();

                    if (!nameLookup.Contains(name) && !name.StartsWith("PRB"))
                    {
                        if (nameLookup.Contains(searchref))
                        {
                            name = searchref;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    _ = int.TryParse(dbfRecord.Values[iSalesOrder].ToString(), out int SalesOrder);
                    _ = int.TryParse(dbfRecord.Values[iStockQuantity].ToString(), out int Stock);
                    _ = int.TryParse(dbfRecord.Values[iPurchaseOrder].ToString(), out int OnOrder);
                    _ = int.TryParse(dbfRecord.Values[iSell].ToString(), out int Sell);
                    _ = int.TryParse(dbfRecord.Values[iCost].ToString(), out int Cost);

                    SalesOrder /= 100;
                    Stock /= 100;
                    OnOrder /= 100;

                    OperaFields record = new()
                    {
                        StockReference = name,
                        QtyInStock = Stock,
                        QtyPurchaseOrder = OnOrder,
                        QtySalesOrder = SalesOrder,
                        SellPrice = Sell,
                        CostPrice = Cost
                    };

                    if (record.StockReference.StartsWith("PRB"))
                    {
                        bar_records.Add(record);
                    }
                    else
                    {
                        results.Add(record);
                    }
                }
            }

            total_records = results.Count;
            i = 0;
            Console.WriteLine();

            foreach (OperaFields r in results)
            {
                i++;
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;
                Console.Write($"\rUpdating Lighthouse... [  {percent_progress:#.00}%  ]");

                TurnedProduct productRecord = products.Find(x => x.ProductName == r.StockReference);
                if (productRecord == null)
                {
                    continue;
                }

                if (RecordNeedsUpdating(productRecord, r))
                {
                    productRecord.QuantityInStock = r.QtyInStock;
                    productRecord.QuantityOnPO = r.QtyPurchaseOrder;
                    productRecord.QuantityOnSO = r.QtySalesOrder;
                    productRecord.SellPrice = r.SellPrice;

                    DatabaseHelper.Update<TurnedProduct>(productRecord);
                }
            }

            total_records = bar_records.Count;
            i = 0;
            Console.WriteLine();

            foreach (OperaFields r in bar_records)
            {
                i++;
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;

                Console.Write($"\rUpdating Bar Stock records... [  {percent_progress:#.00}%  ]");

                BarStock bar_record = barstock.Find(x => x.Id == r.StockReference);
                if (bar_record == null)
                    continue;

                bar_record.InStock = Convert.ToDouble(r.QtyInStock);
                bar_record.OnOrder = Convert.ToDouble(r.QtyPurchaseOrder);
                bar_record.Cost = r.CostPrice;

                DatabaseHelper.Update<BarStock>(bar_record);
            }
        }

        private static bool RecordNeedsUpdating(TurnedProduct product, OperaFields operaRecord)
        {
            return
                product.QuantityInStock != operaRecord.QtyInStock ||
                product.QuantityOnPO != operaRecord.QtyPurchaseOrder ||
                product.QuantityOnSO != operaRecord.QtySalesOrder ||
                product.SellPrice != operaRecord.SellPrice;
        }

        protected class OperaFields
        {
            public int CostPrice { get; set; }
            public int QtyInStock { get; set; }
            public int QtyPurchaseOrder { get; set; }
            public int QtySalesOrder { get; set; }
            public int SellPrice { get; set; }
            public string StockReference { get; set; }
        }

        protected class PurchaseHeader
        {
            public string Reference { get; set; }
            public bool IsClosed { get; set; }
            public bool IsCancelled { get; set; }
        }


        protected class PurchaseLine
        {
            public string PurchaseReference { get; set; }
            public string Account { get; set; }
            public string Product { get; set; }
            public int RequiredQuantity { get; set; }
            public int ReceivedQuantity { get; set; }
        }
    }
}