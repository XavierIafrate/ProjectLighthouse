using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;

namespace LighthouseOperaSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lighthouse-Opera Sync");
#if DEBUG
            DatabaseHelper.DatabasePath = "\\\\groupfile01\\Sales\\Production\\Administration\\Manufacture Records\\Lighthouse_TEST\\lightroom_TEST.db3";
#else
            DatabaseHelper.DatabasePath = @"\\GBCH-LIGHT-01\Lighthouse\lightroom_v2.db3";
#endif
     
            if (!File.Exists(OperaHelper.stockTable))
            {
                Console.WriteLine("Failed to locate db");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Database Found, proceeding.");
            Console.WriteLine("Database Path: " + DatabaseHelper.DatabasePath);

            Program.BackupDb();

            List<TurnedProduct> turnedProductList = DatabaseHelper.Read<TurnedProduct>();
            List<NonTurnedItem> nonTurnedProductList = DatabaseHelper.Read<NonTurnedItem>();
            List<BarStock> barStock = DatabaseHelper.Read<BarStock>();

            string[] array = turnedProductList.Select(x => x.ProductName).ToArray<string>();
            OperaHelper.UpdateRecords(turnedProductList, barStock, nonTurnedProductList);

            OperaHelper.UpdatePurchaseRecords(barStock);
        }

        private static void BackupDb()
        {
            string destFileName = string.Format(@"\\GBCH-LIGHT-01\Lighthouse\bup\bup_{0:yyyyMMddHHmmss}.db3", (object)DateTime.Now);
            try
            {
                File.Copy(DatabaseHelper.DatabasePath, destFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in backup procedure:");
                Console.WriteLine(ex.Message);
            }


            foreach (string file in Directory.GetFiles(@"\\GBCH-LIGHT-01\Lighthouse\bup\"))
            {
                if (Path.GetFileName(file).StartsWith("bup"))
                {
                    if (File.GetLastWriteTime(file).AddDays(14.0) < DateTime.Now)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in deleting old backup:");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
