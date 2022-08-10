using Newtonsoft.Json;
using ProjectLighthouse.Model;
using System;
using System.Diagnostics;
using System.IO;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class LabelPrintingHelper
    {
        public static void TestPrinter()
        {
            InventoryLabel label = new()
            {
                Product = "P0137.200-080",
                GTIN = "05063055585620",
                Location = "location_name_test",
                Tote = "tote_name_test",
                Copies = 1
            };

            Print(label);

        }

        public static void PrintLot(Lot lot)
        {
            LotLabel label = new(lot);
            Print(label);

        }

        static void Print(ILabel label)
        {
            if(!label.DataIsComplete())
            {
                Debug.WriteLine("Data not complete for label");
                return;
            }

            string labelJson = JsonConvert.SerializeObject(label);
            string url = $@"print\l_{DateTime.Now:yyyyMMddHHmmss}.json";
            url = App.ROOT_PATH + url;
            File.WriteAllText(url, labelJson);
        }

        public class LotLabel : ILabel
        {
            public string Product { get; set; }
            public string Batch { get; set; }
            public int Quantity { get; set; }
            public int Copies { get; set; } = 1;
            public string User { get; set; }
            public DateTime Date { get; set; }
            public int TemplateId { get => 1; set => throw new AccessViolationException(); }


            public bool DataIsComplete()
            {
                return !string.IsNullOrEmpty(Product) && Quantity > 0;
            }

            public LotLabel()
            {
                User = App.CurrentUser.UserName;
                Date = DateTime.Now;
            }

            public LotLabel(Lot lot)
            {
                Product = lot.ProductName;
                Batch = lot.MaterialBatch;
                Quantity = lot.Quantity;
                User = App.CurrentUser.UserName;
                Date = DateTime.Now;
            }
        }

        public class InventoryLabel : ILabel
        {
            public string Product { get; set; }
            public string GTIN { get; set; }
            public string Location { get; set; }
            public string Tote { get; set; }
            public int Copies { get; set; } = 1;
            public string User { get; set; }
            public DateTime Date { get; set; }
            public int TemplateId { get => 3; set => throw new AccessViolationException(); }

            public InventoryLabel()
            {
                User = App.CurrentUser.UserName;
                Date = DateTime.Now;
            }

            public bool DataIsComplete()
            {
                return !string.IsNullOrEmpty("Product") && !string.IsNullOrEmpty(GTIN) && !string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(Tote) && Copies > 0;
            }
        }


        public interface ILabel
        {
            int TemplateId { get; set; }
            int Copies { get; set; }

            bool DataIsComplete();
        }
    }
}
