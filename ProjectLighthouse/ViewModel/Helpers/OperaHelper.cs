﻿using DbfDataReader;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class OperaHelper
    {
        public static readonly string dbFile = @"\\groupdb01\O3 Server VFP Static and Dynamic\Data\AUTO\a_cname.dbf";

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



        public static async Task<int> UpdateStockLevelsAsync(IProgress<int> progress)
        {
            //int totalCount = imageList.Count;
            int processCount = await Task.Run<int>(() =>
            {
                int tempCount = 0;
                //foreach (var image in imageList)
                //{
                //    //await the processing and uploading logic here
                //    //int processed = await UploadAndProcessAsync(image);
                //    if (progress != null)
                //    {
                //        //progress.Report((tempCount * 100 / totalCount));
                //    }
                //    tempCount++;
                //}
                return tempCount;
            });
            return processCount;

            //if (!File.Exists(dbFile))
            //{
            //    MessageBox.Show("Cannot locate database.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();
            //string[] lookup = new string[products.Count];

            //for (int i = 0; i < products.Count; i++)
            //    lookup[i] = products[i].ProductName;

            //await Task.Run(() => UpdateRecords(products, lookup));
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
    }
}