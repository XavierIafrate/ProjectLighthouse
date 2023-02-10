using DbfDataReader;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Web.WebView2.Core;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
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
                errs.Add($"No lines found for {item} on {order}.");
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

        private static List<BomComponent> GetComponentStructure()
        {
            List<BomComponent> boms = new();

            string path = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_cstruc.dbf";
            using DbfTable dbfTable = new(path, Encoding.UTF8);

            DbfRecord dbfRecord = new(dbfTable);

            while (dbfTable.Read(dbfRecord))
            {
                if (dbfRecord.IsDeleted)
                {
                    continue;
                }
                object qty = dbfRecord.Values[2]?.GetValue();
                int quantity = 0;

                if (qty is not null)
                {
                    quantity = Convert.ToInt32(qty.ToString());
                }

                quantity /= 100;

                boms.Add(new() { AssemblyName = dbfRecord.Values[0].ToString(), ComponentName = dbfRecord.Values[1].ToString(), Quantity = quantity });
            }

            return boms;
        }

        private static void UpdateProduct(TurnedProduct product, OperaFields record)
        {
            if (!RecordNeedsUpdating(product, record))
            {
                return;
            }

            product.QuantityInStock = record.QtyInStock;
            product.QuantityOnPO = record.QtyPurchaseOrder;
            product.QuantityOnSO = record.QtySalesOrder;
            product.SellPrice = record.SellPrice;

            DatabaseHelper.Update<TurnedProduct>(product);
        }

        private static void UpdateBarStock(BarStock bar, OperaFields record)
        {
            bar.InStock = Convert.ToDouble(record.QtyInStock);
            bar.OnOrder = Convert.ToDouble(record.QtyPurchaseOrder);
            bar.Cost = record.CostPrice;

            DatabaseHelper.Update<BarStock>(bar);
        }

        public static void UpdateRecords(List<TurnedProduct> products, List<BarStock> barstock, string[] lighthouseProducts)
        {
            int total_records;
            int i;

            List<OperaFields> liveData;

            List<BomComponent> boms = GetComponentStructure();

            liveData = GetLiveData(lighthouseProducts, barstock.Select(x => x.Id).ToArray(), boms);
            
            total_records = liveData.Count;
            i = 0;
            Console.WriteLine();


            foreach (OperaFields record in liveData)
            {
                i++;
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;
                Console.Write($"\rUpdating Lighthouse... [  {percent_progress:#.00}%  ]");

                TurnedProduct productRecord = products.Find(x => x.ProductName == record.StockReference);
                if (productRecord is not null)
                {
                    UpdateProduct(productRecord, record);
                    continue;
                }

                BarStock bar = barstock.Find(x => x.Id == record.StockReference);
                if (bar is not null)
                {
                    UpdateBarStock(bar, record);
                    continue;
                }
            }

        }

        private static List<OperaFields> GetLiveData(string[] lighthouseProducts, string[] barstock, List<BomComponent> boms)
        {
            boms = boms.Where(x => lighthouseProducts.Contains(x.ComponentName)).ToList();

            string[] itemsForUpdate = lighthouseProducts.Concat(barstock).ToArray();

            List<OperaFields> results = new();
            List<OperaFields> bar_records = new();

            using DbfTable dbfTable = new(dbFile, Encoding.UTF8);

            DbfHeader header = dbfTable.Header;
            Console.WriteLine($"{header.RecordCount:#,##0} records in Automotion CNAME table.");

            int iStockRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_REF"));
            int iFactor = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_FACT"));
            int iSearchRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_CAT"));
            int iSalesOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SALEORD"));
            int iStockQuantity = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_INSTOCK"));
            int iPurchaseOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_ONORDER"));
            int iSell = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SELL"));
            int iCost = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_COST"));

            DbfRecord dbfRecord = new(dbfTable);

            int total_records = (int)header.RecordCount;
            int i = 0;

            while (dbfTable.Read(dbfRecord))
            {
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;

                Console.Write($"\rReading Opera... [  {percent_progress:#.00}%  ]");

                i++;

                if (dbfRecord.IsDeleted)
                    continue;

                string name = dbfRecord.Values[iStockRef].ToString() ?? "";
                string factor = dbfRecord.Values[iFactor].ToString();
                string searchref = dbfRecord.Values[iSearchRef].ToString();

                if (!itemsForUpdate.Contains(name))
                {
                    if (itemsForUpdate.Contains(searchref))
                    {
                        name = searchref;
                    }
                    else if (boms.Any(x => x.AssemblyName == name))
                    {
                        // proceed normally
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
                    SellPrice = factor == "4DEC" ? Sell / 100 : Sell,
                    CostPrice = Cost
                };

                results.Add(record);
            }



            // Consolidate Bom Records
            List<OperaFields> assemblies = results.Where(x => !itemsForUpdate.Contains(x.StockReference)).ToList();

            // remove assemblies
            results = results.Where(x => itemsForUpdate.Contains(x.StockReference)).ToList();

            for (int j = 0; j < assemblies.Count; j++)
            {
                OperaFields assembly = assemblies[j];
                List<BomComponent> affectedComponents = boms.Where(x => x.AssemblyName == assembly.StockReference).ToList();

                for (int k = 0; k < affectedComponents.Count; k++)
                {
                    OperaFields? lighthouseComponent = results.Find(x => x.StockReference == affectedComponents[k].ComponentName);
                    if (lighthouseComponent is null) continue;

                    Console.WriteLine($"\t{assembly.StockReference} >> '{lighthouseComponent.StockReference}' +{assembly.QtySalesOrder}");
                    lighthouseComponent.QtySalesOrder += assembly.QtySalesOrder;
                }
            }

            return results;
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

        protected class BomComponent
        {
            public string AssemblyName { get; set; }
            public string ComponentName { get; set; }
            public int Quantity { get; set; }
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