using DbfDataReader;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class OperaHelper
    {
        public Dictionary<string, string> TablePaths;

        public OperaHelper(string applicationRoot)
        {
            string fileRead;
            try
            {
                fileRead = File.ReadAllText(Path.Join(applicationRoot, "opera_tables.json"));
            }
            catch(Exception ex)
            {
                throw new Exception($"An exception occurred when trying to read from opera_tables.json: {ex.Message}");
            }

            Dictionary<string, string> deserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(fileRead);

            string[] expectedTables = new string[] { "CNAME", "DOLINE", "DOHEAD", "ITRAN", "CSTRUC", "CFACT" };

            foreach (string table in expectedTables)
            {
                if(!deserialised.ContainsKey(table))
                {
                    throw new Exception($"opera_tables.json does not contain key for {table}");
                }
            }

            TablePaths = deserialised;
        }

        public List<string> VerifyDeliveryNote(List<DeliveryItem> items)
        {
            List<string> errs = new();

            string[] relevantPurchases = items.Select(x => x.PurchaseOrderReference.ToUpperInvariant()).Distinct().ToArray();

            List<PurchaseLine> lines = GetPurchaseLines(relevantPurchases);
            //List<PurchaseHeader> headers = GetPurchaseHeaders(relevantPurchases);

            foreach (string order in relevantPurchases)
            {
                List<DeliveryItem> itemsOnPO = items.Where(x => x.PurchaseOrderReference == order).ToList();
                string[] uniqueItems = itemsOnPO.Select(x => x.ExportProductName).Distinct().ToArray();

                foreach (string item in uniqueItems)
                {
                    List<DeliveryItem> targets = itemsOnPO.Where(x => x.ExportProductName == item).ToList();
                    int qtyThisDel = targets.Sum(x => x.QuantityThisDelivery);
                    errs.AddRange(VerifyData(item, qtyThisDel, order, lines));
                }

                if (itemsOnPO.Count == 0)
                {
                    errs.Add($"No lines found for purchase order '{order}'");
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

        private List<PurchaseLine> GetPurchaseLines(string[] refs)
        {
            List<PurchaseLine> results = new();

            using DbfTable dbfTable = new(TablePaths["DOLINE"], Encoding.UTF8);
            DbfHeader tableHeader = dbfTable.Header;
            Console.WriteLine($"{tableHeader.RecordCount:#,##0} records in Purchase Order Lines table.");

            int iRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_DCREF"));
            int iAccount = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_ACCOUNT"));
            int iPartNumber = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_CNREF"));
            int iRequired = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_REQQTY"));
            int iReceived = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_RECQTY"));
            int iFunDec = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_FUNDEC"));

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
                string poRef = dbfRecord.Values[iRef].ToString() ?? "";
                if (!refs.Contains(poRef.ToUpperInvariant()))
                {
                    continue;
                }

                string partNum = dbfRecord.Values[iPartNumber].ToString();

                _ = int.TryParse(dbfRecord.Values[iRequired].ToString(), out int required);
                _ = int.TryParse(dbfRecord.Values[iReceived].ToString(), out int received);
                _ = int.TryParse(dbfRecord.Values[iFunDec].ToString(), out int decPlaces);

                int divisor = decPlaces == 4 ? 1 : 100;

                required /= divisor;
                received /= divisor;

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

        // Main Sync Entry
        public void UpdateRecords(List<TurnedProduct> products, List<BarStock> barstock, List<NonTurnedItem> nonTurnedItems)
        {
            List<BomComponent> boms = GetComponentStructure();
            List<StockFactor> factors = GetFactors();

            List<OperaStockData> operaData = GetLiveData(factors);


            Console.WriteLine($"\nUpdating Records");


            for (int i = 0; i < products.Count; i++)
            {
                TurnedProduct product = products[i];
                OperaStockData? liveData = operaData.Find(x => x.AutomotionRef == product.ProductName);
                liveData ??= operaData.Find(x => x.WixroydRef == product.ProductName);
                liveData ??= operaData.Find(x => x.TeknipartRef == product.ProductName);
                liveData ??= operaData.Find(x => x.GTIN == product.ProductName);

                if (liveData == null)
                {
                    Console.WriteLine($"No stock record found for turned item '{product.ProductName}'");
                    product.IsSyncing = false;

                    if (product.IsSyncing == false) continue;

                    try
                    {
                        DatabaseHelper.ExecuteCommand($"UPDATE {nameof(TurnedProduct)} SET IsSyncing = {false}, ExportProductName=NULL, QuantityOnPO=0, QuantityOnSO=0, QuantityInStock=0 WHERE ProductName='{product.ProductName}'");
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine($"Failed to set IsSyncing to false for turned item: {err.Message}");
                    }

                    continue;
                }

                if (!RecordNeedsUpdating(product, liveData))
                {
                    continue;
                }

                try
                {
                    DatabaseHelper.ExecuteCommand($"UPDATE {nameof(TurnedProduct)} SET ExportProductName = '{liveData.GTIN}', IsSyncing={true} WHERE ProductName='{product.ProductName}'");
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Failed to set IsSyncing to true and update GTIN for turned item: {err.Message}");
                }

                try
                {
                    DatabaseHelper.ExecuteCommand($"UPDATE {nameof(TurnedProduct)} SET QuantityInStock={liveData.QtyInStock}, QuantityOnPO={liveData.QtyPurchaseOrder}, QuantityOnSO={liveData.QtySalesOrder}, SellPrice={liveData.SellPrice}  WHERE ProductName='{product.ProductName}'");
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Failed to update turned item: {err.Message}");
                }
            }

            for (int i = 0; i < barstock.Count; i++)
            {
                BarStock bar = barstock[i];
                OperaStockData? liveData = operaData.Find(x => x.AutomotionRef == bar.Id);
                liveData ??= operaData.Find(x => x.WixroydRef == bar.Id);
                liveData ??= operaData.Find(x => x.TeknipartRef == bar.Id);
                liveData ??= operaData.Find(x => x.GTIN == bar.Id);

                if (liveData == null)
                {
                    Console.WriteLine($"No stock record found for bar '{bar.Id}'");
                    bar.IsSyncing = false;

                    try
                    {
                        DatabaseHelper.ExecuteCommand($"UPDATE {nameof(BarStock)} SET IsSyncing = {false}, ErpId=NULL, OnOrder=0, InStock=0 WHERE Id='{bar.Id}'");
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine($"Failed to set IsSyncing to false for bar: {err.Message}");
                    }

                    continue;
                }

                try
                {
                    DatabaseHelper.ExecuteCommand($"UPDATE {nameof(BarStock)} SET ErpId = '{liveData.GTIN}', IsSyncing={true}, InStock={liveData.QtyInStock}, OnOrder={liveData.QtyPurchaseOrder}, Cost={liveData.CostPrice}  WHERE Id='{bar.Id}'");
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Failed to set IsSyncing to true and update GTIN for bar: {err.Message}");
                }
            }

            for (int i = 0; i < nonTurnedItems.Count; i++)
            {
                NonTurnedItem item = nonTurnedItems[i];
                OperaStockData? liveData = operaData.Find(x => x.AutomotionRef == item.Name);
                liveData ??= operaData.Find(x => x.WixroydRef == item.Name);
                liveData ??= operaData.Find(x => x.TeknipartRef == item.Name);
                liveData ??= operaData.Find(x => x.GTIN == item.Name);

                if (liveData == null)
                {
                    Console.WriteLine($"No stock record found for non-turned item '{item.Name}'");
                    item.IsSyncing = false;

                    try
                    {
                        DatabaseHelper.ExecuteCommand($"UPDATE {nameof(NonTurnedItem)} SET IsSyncing = {false}, ExportProductName=NULL WHERE Name='{item.Name}'");
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine($"Failed to set IsSyncing to false for non-turned item: {err.Message}");
                    }

                    continue;
                }

                try
                {
                    DatabaseHelper.ExecuteCommand($"UPDATE {nameof(NonTurnedItem)} SET ExportProductName='{liveData.GTIN}', IsSyncing={true} WHERE Name='{item.Name}'");
                }
                catch (Exception err)
                {
                    Console.WriteLine($"Failed to set IsSyncing to true and update GTIN for non-turned item: {err.Message}");
                }
            }


            //string[] uniqueStockReferences = liveData.Select(x => x.StockReference).Distinct().ToArray();

            //List<OperaStockData> cleanedLiveData = new();
            //foreach (string stockReference in uniqueStockReferences)
            //{
            //    List<OperaStockData> targets = liveData.Where(x => x.StockReference == stockReference).ToList();

            //    OperaStockData record = new()
            //    {

            //        StockReference = stockReference,
            //        QtyInStock = targets.Where(x => !x.searchRef).Sum(x => x.QtyInStock),
            //        QtyPurchaseOrder = targets.Sum(x => x.QtyPurchaseOrder),
            //        QtySalesOrder = targets.Sum(x => x.QtySalesOrder),
            //        SellPrice = targets.Max(x => x.SellPrice),
            //        CostPrice = targets.Max(x => x.CostPrice)
            //    };

            //    cleanedLiveData.Add(record);
            //}

            //total_records = liveData.Count;
            //i = 0;
            //Console.WriteLine();


            //foreach (OperaFields record in cleanedLiveData)
            //{
            //    i++;
            //    double percent_progress = (double)i / (double)total_records;
            //    percent_progress *= 100;
            //    Console.Write($"\rUpdating Lighthouse... [  {percent_progress:#.00}%  ]");

            //    TurnedProduct productRecord = products.Find(x => x.ProductName == record.StockReference || x.ExportProductName == record.StockReference);

            //    if (productRecord is not null)
            //    {
            //        UpdateProduct(productRecord, record);
            //        continue;
            //    }

            //    BarStock bar = barstock.Find(x => x.Id == record.StockReference);
            //    if (bar is not null)
            //    {
            //        UpdateBarStock(bar, record);
            //        continue;
            //    }
            //}

            WriteTimeStamp();
        }

        private List<StockFactor> GetFactors()
        {
            List<StockFactor> factors = new();

            using DbfTable dbfTable = new(TablePaths["CFACT"], Encoding.UTF8);
            DbfRecord dbfRecord = new(dbfTable);

            int iCode = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CF_CODE"));
            int iSellDps = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CF_SELLDPS"));

            while (dbfTable.Read(dbfRecord))
            {
                if (dbfRecord.IsDeleted)
                {
                    continue;
                }

                string code = dbfRecord.GetStringValue(iCode);
                int dps = dbfRecord.GetValue<int>(iSellDps);

                factors.Add(new() { Code = code, Dps = dps });

            }

            return factors;
        }

        private List<BomComponent> GetComponentStructure()
        {
            List<BomComponent> boms = new();

            using DbfTable dbfTable = new(TablePaths["CSTRUC"], Encoding.UTF8);

            DbfRecord dbfRecord = new(dbfTable);

            while (dbfTable.Read(dbfRecord))
            {
                if (dbfRecord.IsDeleted)
                {
                    continue;
                }

                int quantity = dbfRecord.GetValue<int>(2);

                quantity /= 100;

                boms.Add(new() { AssemblyName = dbfRecord.Values[0].ToString(), ComponentName = dbfRecord.Values[1].ToString(), Quantity = quantity });
            }

            return boms;
        }



        private static void WriteTimeStamp()
        {
            ApplicationVariable timeStamp = new() { Id = "SYNC_LAST_RUN", Data = DateTime.Now };

            try
            {
                DatabaseHelper.Insert(timeStamp, throwErrs: true);
            }
            catch
            {
                DatabaseHelper.Update(timeStamp, throwErrs: false);
            }
        }

        private List<OperaStockData> GetLiveData(List<StockFactor> factors)
        {
            List<OperaStockData> results = new();

            using DbfTable dbfTable = new(TablePaths["CNAME"], Encoding.UTF8);

            DbfHeader header = dbfTable.Header;
            Console.WriteLine($"{header.RecordCount:#,##0} records in Opera Stock table.");

            int iGtin = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_REF"));
            int iAutomotionRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "OACN_AREF"));
            int iWixroydRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "OACN_WREF"));
            int iTeknipartRef = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "OACN_TREF"));
            int iFactor = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_FACT"));
            int iSalesOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SALEORD"));
            int iStockQuantity = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_INSTOCK"));
            int iPurchaseOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_ONORDER"));
            int iSell = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_SELL"));
            int iCost = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_LCOST"));
            int iDormant = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "CN_DORMANT"));

            DbfRecord dbfRecord = new(dbfTable);

            int total_records = (int)header.RecordCount;
            int i = 0;

            while (dbfTable.Read(dbfRecord))
            {
                double percent_progress = (double)i / (double)total_records;
                percent_progress *= 100;
                Console.Write($"\rReading database... [  {percent_progress:#0.00}%  ]");
                i++;

                if (dbfRecord.IsDeleted)
                    continue;

                string aStockRef = dbfRecord.GetStringValue(iAutomotionRef);
                string wStockRef = dbfRecord.GetStringValue(iWixroydRef);
                string tStockRef = dbfRecord.GetStringValue(iTeknipartRef);
                string gtin = dbfRecord.GetStringValue(iGtin);
                string factor = dbfRecord.GetStringValue(iFactor);

                StockFactor? stockFactor = factors.Find(x => x.Code == factor);

                if (stockFactor == null)
                {
                    // weird
                    continue;
                }

                bool dormant = true;
                if (dbfRecord.GetValue(iDormant) is not null)
                {
                    dormant = dbfRecord.GetValue<bool>(iDormant);
                }

                int qtySalesOrder = (dbfRecord.GetValue(iSalesOrder) is not null) ?  dbfRecord.GetValue<int>(iSalesOrder) : 0;
                int qtyInStock = (dbfRecord.GetValue(iStockQuantity) is not null) ? dbfRecord.GetValue<int>(iStockQuantity) : 0;
                int qtyPurchaseOrder = (dbfRecord.GetValue(iPurchaseOrder) is not null) ? dbfRecord.GetValue<int>(iPurchaseOrder) : 0;
                int sellPrice = (dbfRecord.GetValue(iSell) is not null) ? dbfRecord.GetValue<int>(iSell) : 0;
                int costPrice = (dbfRecord.GetValue(iCost) is not null) ? dbfRecord.GetValue<int>(iCost) : 0;

                qtySalesOrder /= 100;
                qtyInStock /= 100;
                qtyPurchaseOrder /= 100;
                double dSellPrice = Convert.ToDouble(sellPrice) / Math.Pow(10, Convert.ToDouble(stockFactor.Dps));

                OperaStockData record = new()
                {
                    AutomotionRef = aStockRef,
                    WixroydRef = wStockRef,
                    TeknipartRef = tStockRef,
                    GTIN = gtin,
                    QtyInStock = qtyInStock,
                    QtyPurchaseOrder = qtyPurchaseOrder,
                    QtySalesOrder = qtySalesOrder,
                    SellPrice = sellPrice,
                    CostPrice = costPrice,
                    IsDormant = dormant,
                };

                results.Add(record);
            }



            //// Consolidate Bom Records
            //List<OperaFields> assemblies = results.Where(x => !itemsForUpdate.Contains(x.StockReference)).ToList();

            //// remove assemblies
            //results = results.Where(x => itemsForUpdate.Contains(x.StockReference)).ToList();

            //for (int j = 0; j < assemblies.Count; j++)
            //{
            //    OperaFields assembly = assemblies[j];
            //    List<BomComponent> affectedComponents = boms.Where(x => x.AssemblyName == assembly.StockReference).ToList();

            //    for (int k = 0; k < affectedComponents.Count; k++)
            //    {
            //        OperaFields? lighthouseComponent = results.Find(x => x.StockReference == affectedComponents[k].ComponentName);
            //        if (lighthouseComponent is null) continue;

            //        Console.WriteLine($"\t{assembly.StockReference} >> '{lighthouseComponent.StockReference}' +{assembly.QtySalesOrder}");
            //        lighthouseComponent.QtySalesOrder += assembly.QtySalesOrder;
            //    }
            //}

            return results;
        }

        private static bool RecordNeedsUpdating(TurnedProduct product, OperaStockData operaRecord)
        {
            return
                product.ExportProductName != operaRecord.GTIN ||
                product.IsSyncing != true ||
                product.QuantityInStock != operaRecord.QtyInStock ||
                product.QuantityOnPO != operaRecord.QtyPurchaseOrder ||
                product.QuantityOnSO != operaRecord.QtySalesOrder ||
                product.SellPrice != operaRecord.SellPrice;
        }

        protected class OperaStockData
        {
            public int CostPrice { get; set; }
            public int QtyInStock { get; set; }
            public int QtyPurchaseOrder { get; set; }
            public int QtySalesOrder { get; set; }
            public int SellPrice { get; set; }
            public string AutomotionRef { get; set; }
            public string WixroydRef { get; set; }
            public string TeknipartRef { get; set; }
            public string GTIN { get; set; }
            public bool IsDormant { get; set; }
        }

        protected class StockFactor
        {
            public string Code { get; set; }
            public int Dps { get; set; }
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

        public class InvoiceLine
        {
            [OperaColumn("it_numinv")]
            public string InvoiceNumber { get; set; }

            [OperaColumn("it_dteinv")]
            public DateTime InvoiceDate { get; set; }

            [OperaColumn("it_stock")]
            public string AccountNumber { get; set; }

            [OperaColumn("it_stock")]
            public string ProductName { get; set; }

            [OperaColumn("it_quan")]
            public int LineQuantity { get; set; }

            [OperaColumn("it_status")]
            public string Status { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class OperaColumn : Attribute
        {
            public string ColumnName { get; set; }
            public OperaColumn(string name)
            {
                ColumnName = name.ToLowerInvariant();
            }
        }

        public static List<InvoiceLine> GetInvoiceLines(string path)
        {
            IEnumerable<PropertyInfo> props = typeof(InvoiceLine).GetProperties().Where(
                x => Attribute.IsDefined(x, typeof(OperaColumn)));

            using DbfTable dbfTable = new(path, Encoding.UTF8);
            Dictionary<PropertyInfo, int> map = GetInvoiceColumnMap(dbfTable, props);

            DbfHeader tableHeader = dbfTable.Header;
            List<InvoiceLine> invoiceLines = new();
            DbfRecord dbfRecord = new(dbfTable);

            while (dbfTable.Read(dbfRecord))
            {

                if (dbfRecord.IsDeleted)
                {
                    continue;
                }

                InvoiceLine newInvoiceLine = GetInvoiceLine(dbfRecord, map);

                if (newInvoiceLine.Status == "X" && !string.IsNullOrEmpty(newInvoiceLine.InvoiceNumber))
                {
                    invoiceLines.Add(newInvoiceLine);
                }
            }

            return invoiceLines;

        }
        private static InvoiceLine GetInvoiceLine(DbfRecord record, Dictionary<PropertyInfo, int> map)
        {
            InvoiceLine x = new();

            for (int i = 0; i < map.Count; i++)
            {
                PropertyInfo prop = map.Keys.ElementAt(i);
                Type propType = prop.PropertyType;
                int j = map[prop];
                object cell = record.Values.ElementAt(j).GetValue();
                dynamic value;

                try
                {
                    value = Convert.ChangeType(cell, propType);
                }
                catch (InvalidCastException)
                {
                    value = GetDefault(propType);
                }

                x.GetType().GetProperty(prop.Name)!.SetValue(x, value);
            }

            return x;
        }

        private static Dictionary<PropertyInfo, int> GetInvoiceColumnMap(DbfTable dbfTable, IEnumerable<PropertyInfo> props)
        {
            Dictionary<PropertyInfo, int> m = new();

            for (int j = 0; j < props.Count(); j++)
            {
                PropertyInfo x = props.ElementAt(j);
                OperaColumn attr = x.GetCustomAttribute<OperaColumn>();
                int colIndex = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName.ToLowerInvariant() == attr!.ColumnName));

                m.Add(x, colIndex);
            }

            return m;
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public void UpdatePurchaseRecords(List<BarStock> barStock)
        {
            string[] barIds = barStock.Select(x => x.ErpId).ToArray();
            List<BarStockPurchase> existingRecords = DatabaseHelper.Read<BarStockPurchase>();
            List<BarStockPurchase> purchaseLines = GetBarPurchaseLines(barIds);

            foreach (BarStockPurchase newData in purchaseLines)
            {
                newData.BarId = barStock.Find(x => x.ErpId == newData.BarId)?.Id;

                BarStockPurchase? existingData = existingRecords.Find(x => x.OperaId == newData.OperaId && x.Id > 518); // 518 records before change of database for Opera

                if (existingData is null)
                {
                    DatabaseHelper.Insert(newData);
                    continue;
                }

                if (existingData.IsUpdated(newData))
                {
                    newData.Id = existingData.Id;
                    DatabaseHelper.Update(newData);
                }
            }
        }

        private List<BarStockPurchase> GetBarPurchaseLines(string[] refs)
        {
            List<BarStockPurchase> results = new();

            using DbfTable dbfTable = new(TablePaths["DOLINE"], Encoding.UTF8);
            DbfHeader tableHeader = dbfTable.Header;
            Console.WriteLine($"{tableHeader.RecordCount:#,##0} records in Purchase Order Lines table.");

            int iPurchaseOrder = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_DCREF"));
            int iOperaId = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "ID"));
            int iAccount = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_ACCOUNT"));
            int iPartNumber = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_CNREF"));
            int iRequired = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_REQQTY"));
            int iRequiredDate = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_REQDAT"));
            int iReceived = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_RECQTY"));
            int iIsQuoted = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_QUOTED"));
            int iQuotedDate = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_QUODAT"));
            int iValue = dbfTable.Columns.IndexOf(dbfTable.Columns.Single(n => n.ColumnName == "DO_VALUE"));

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
                string partNum = dbfRecord.GetStringValue(iPartNumber);

                if (!refs.Contains(partNum.ToUpperInvariant()))
                {
                    continue;
                }

                string accountRef = dbfRecord.GetStringValue(iAccount);
                string poRef = dbfRecord.GetStringValue(iPurchaseOrder);
                int required = dbfRecord.GetValue<int>(iRequired);
                int received = dbfRecord.GetValue<int>(iReceived);
                Int64 id = dbfRecord.GetValue<Int64>(iOperaId);
                double lineValue = Convert.ToDouble(dbfRecord.GetValue<Int64>(iValue));
                DateTime requiredDate = dbfRecord.GetValue<DateTime>(iRequiredDate);
                DateTime? quotedDate = null;
                if (dbfRecord.GetValue(iQuotedDate) is not null)
                {
                    quotedDate = dbfRecord.GetValue<DateTime>(iQuotedDate);
                }
                bool isQuoted = dbfRecord.GetValue<bool>(iIsQuoted);

                required /= 100;
                received /= 100;
                lineValue /= 100;

                BarStockPurchase purchase = new()
                {
                    OperaId = id,
                    BarId = partNum,
                    LineValue = lineValue,
                    QuantityRequired = required,
                    QuantityReceived = received,
                    PurchaseOrder = poRef,
                    SupplierAccount = accountRef,

                    DateRequired = requiredDate,
                    QuotedDate = quotedDate,
                    IsQuoted = isQuoted,
                };

                results.Add(purchase);
            }

            return results;
        }
    }
}