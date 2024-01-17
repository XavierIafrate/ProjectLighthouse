using CsvHelper.Configuration.Attributes;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ViewModel.Commands.Administration;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class DatabaseManagerViewModel : BaseViewModel
    {
        public ManageUsersViewModel ManageUsersViewModel { get; set; }
        public LatheViewModel LatheViewModel { get; set; }
        public MaterialsViewModel MaterialsViewModel { get; set; }

        public List<LatheManufactureOrder> Orders { get; set; }

        public GetRecordsAsCsvCommand GetRecordsAsCsvCmd { get; set; }

        public DatabaseManagerViewModel()
        {
            GetRecordsAsCsvCmd = new(this);
            Orders = DatabaseHelper.Read<LatheManufactureOrder>();

            if (App.CurrentUser.HasPermission(Model.Core.PermissionType.EditUsers))
            {
                ManageUsersViewModel = new();
            }

            if (App.CurrentUser.HasPermission(Model.Core.PermissionType.EditMaterials))
            {
                MaterialsViewModel = new();
            }

            if (App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageLathes))
            {
                LatheViewModel = new();
            }

        }

        public void GetCsv(string type)
        {
            switch (type)
            {
                case "Orders":
                    CSVHelper.WriteListToCSV(Orders, "orders");
                    break;
                case "ItemCosting":
                    GenerateItemCostingSheet();
                    break;
                case "BarStockEstimate":
                    GetBarInventory();
                    break;

                case "Delivered":
                    GetDeliveredItems();
                    break;
                case "TotalManufactured":
                    UpdateTotalManufactured();
                    break;
                case "ImportTriggers":
                    ImportTriggers();
                    break;
                default:
                    throw new NotImplementedException();
            };
        }

        private static void ImportTriggers()
        {
            string[] data = File.ReadAllLines(@"C:\Users\x.iafrate\Desktop\newTriggers.txt");
            List<TurnedProduct> turnedProducts = DatabaseHelper.Read<TurnedProduct>().ToList();
            Dictionary<string, int> newTriggers = new();

            for (int i = 0; i < data.Length; i++)
            {
                string entry = data[i];
                string[] split = entry.Split(',');
                if (split.Length != 2)
                {
                    Debug.WriteLine($"Length not 2: '{entry}'");
                    continue;
                }

                if (!int.TryParse(split[1], out int trigger))
                {
                    Debug.WriteLine($"Could not parse int: '{split[1]}'");
                    continue;
                }

                newTriggers.Add(split[0], trigger);
            }

            foreach (KeyValuePair<string, int> trigger in newTriggers)
            {
                TurnedProduct? p = turnedProducts.Find(x => x.ProductName == trigger.Key);
                p ??= turnedProducts.Find(x => x.ExportProductName == trigger.Key);

                if (p is null)
                {
                    Debug.WriteLine($"Could not find product: '{trigger.Key}'");
                    continue;
                }

                if (p.QuantitySold == trigger.Value)
                {
                    continue;
                }

                p.QuantitySold = trigger.Value;

                try
                {
                    DatabaseHelper.Update(p, throwErrs: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void UpdateTotalManufactured()
        {
            List<TurnedProduct> turnedProducts = DatabaseHelper.Read<TurnedProduct>().Where(x => !x.Retired).ToList();
            List<Lot> deliveredLots = DatabaseHelper.Read<Lot>().Where(x => x.IsDelivered).ToList();

            foreach (TurnedProduct product in turnedProducts)
            {
                product.ValidateAll();
                if (product.HasErrors) continue;

                int totalDelivered = deliveredLots.Where(x => x.ProductName == product.ProductName).Sum(x => x.Quantity);

                if (totalDelivered == product.QuantityManufactured) continue;

                DatabaseHelper.ExecuteCommand($"UPDATE {nameof(TurnedProduct)} SET QuantityManufactured = {totalDelivered} WHERE Id={product.Id}");
            }
        }

        class DeliveryLine
        {
            public DateTime Date { get; set; }
            public string ProductName { get; set; }
            public string DeliveryNote { get; set; }
            public int Quantity { get; set; }
        }

        private static void GetDeliveredItems()
        {
            List<DeliveryLine> result = new();

            List<DeliveryNote> deliveryNotes = DatabaseHelper.Read<DeliveryNote>().Where(x => x.DeliveryDate.AddYears(2) > DateTime.Now).ToList();
            List<DeliveryItem> deliveryLines = DatabaseHelper.Read<DeliveryItem>();

            foreach (DeliveryNote deliveryNote in deliveryNotes)
            {
                List<DeliveryItem> items = deliveryLines.Where(x => x.AllocatedDeliveryNote == deliveryNote.Name).ToList();

                foreach (DeliveryItem item in items)
                {
                    result.Add(new()
                    {
                        Date = deliveryNote.DeliveryDate,
                        DeliveryNote = deliveryNote.Name,
                        ProductName = string.IsNullOrEmpty(item.ExportProductName)
                            ? item.Product
                            : item.ExportProductName,
                        Quantity = item.QuantityThisDelivery
                    }
                    );
                }
            }

            CSVHelper.WriteListToCSV(result, "delivered", "dd/MM/yyyy");
        }


        public class BarStockInventory
        {
            [CsvHelper.Configuration.Attributes.Name("Bar ID")]
            public string Id { get; set; }
            [CsvHelper.Configuration.Attributes.Name("In Stock")]
            public int InStock { get; set; }
            [CsvHelper.Configuration.Attributes.Name("Estimated On Rack")]
            public int OnRack { get; set; }
        }

        private static void GetBarInventory()
        {
            List<LatheManufactureOrder> activeOrders = DatabaseHelper.Read<LatheManufactureOrder>()
                .Where(x => x.State < OrderState.Complete && x.NumberOfBarsIssued > 0)
                .ToList();

            List<BarStock> barStock = DatabaseHelper.Read<BarStock>().OrderBy(x => x.Id).ToList();
            List<BarStockInventory> result = new();

            foreach (BarStock bar in barStock)
            {
                BarStockInventory x = new() { Id = bar.Id, InStock = (int)bar.InStock };

                List<LatheManufactureOrder> ordersUsingBar = activeOrders.Where(x => x.BarID == bar.Id).ToList();


                int barsOnRack = 0;
                foreach (LatheManufactureOrder order in ordersUsingBar)
                {
                    if (order.State < OrderState.Running)
                    {
                        barsOnRack += order.NumberOfBarsIssued;
                        continue;
                    }

                    double progress = (DateTime.Now - order.StartDate) / (TimeSpan.FromSeconds(order.TimeToComplete) * (order.NumberOfBarsIssued / order.NumberOfBars));

                    int barsConsumed = (int)Math.Floor(Math.Min(progress, 1) * order.NumberOfBarsIssued);
                    barsOnRack += order.NumberOfBarsIssued - barsConsumed;
                }

                x.OnRack = barsOnRack;
                result.Add(x);
            }

            CSVHelper.WriteListToCSV(result, "BarInventory");
        }


        private static void GenerateItemCostingSheet()
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>()
                .Where(x => x.MaterialId is not null && x.GroupId is not null && !x.IsSpecialPart)
                .ToList();

            products.ForEach(x => x.ValidateAll());

            products = products.Where(x => !x.HasErrors).ToList();

            List<BarStock> barStock = DatabaseHelper.Read<BarStock>();
            List<MaterialInfo> materials = DatabaseHelper.Read<MaterialInfo>();
            List<ProductGroup> groups = DatabaseHelper.Read<ProductGroup>();

            List<ItemCost> itemCosts = new();
            foreach (TurnedProduct product in products)
            {
                ProductGroup? memberOf = groups.Find(x => x.Id == product.GroupId);
                MaterialInfo? material = materials.Find(x => x.Id == product.MaterialId);

                if (memberOf is null || material is null)
                {
                    continue;
                }

                List<BarStock> candidates = barStock.Where(x =>
                        x.Size >= (memberOf.MinBarSize ?? memberOf.MajorDiameter)
                        && x.MaterialId == material.Id)
                    .OrderBy(x => x.Size)
                    .ToList();

                if (candidates.Count == 0)
                {
                    continue;
                }

                BarStock bar = candidates.First();

                ItemCost newItem = new(product, bar, material, App.Constants.AbsorptionRate, TimeModel.Default(product.MajorDiameter), 2);

                itemCosts.Add(newItem);
            }

            CSVHelper.WriteListToCSV(itemCosts, "ItemMaterialDetails");

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //string htmlString = "<h1>Costing Analysis</h1>";

            //foreach (ItemCost item in itemCosts)
            //{
            //    htmlString += $"<h4>{item.Product}</h4>";
            //    htmlString += $"<p>Time Cost {item.TimeCost}</p>";
            //    htmlString += $"<p>Material Cost {item.MaterialCost}</p>";
            //    htmlString += $"<p><b>Total Cost {item.TotalCost}</b></p>";
            //}


            //PdfDocument pdfDocument = PdfGenerator.GeneratePdf(htmlString, PdfSharp.PageSize.A4);
            //pdfDocument.Save(@"C:\Users\x.iafrate\Dev\test.pdf");
        }

        public class ItemCost
        {
            public ItemCost(TurnedProduct product, BarStock bar, MaterialInfo materialInfo, double absorptionRate, TimeModel model, double partOff)
            {
                Product = product.ProductName;
                CycleTime = model.At(product.MajorLength);
                TimeCost = CycleTime * absorptionRate;

                Material = materialInfo.MaterialCode;
                CostPerKilo = materialInfo.Cost ?? 0 / 100;
                BarId = bar.Id;
                bar.MaterialData = materialInfo;
                BarLength = bar.Length;
                BarCost = bar.ExpectedCost;
                MaterialBudget = product.MajorLength + product.PartOffLength + partOff;

                MaterialCost = bar.ExpectedCost ?? 0 * (MaterialBudget / (bar.Length - App.Constants.BarRemainder));

                TotalCost = TimeCost + MaterialCost;

                TimeModel = model.ToString();

                HasBeenProduced = product.CycleTime > 0;
            }


            [Name("SKU")]
            public string Product { get; set; }

            [Name("Cycle Time (seconds)")]
            public int CycleTime { get; set; }

            [Name("Material")]
            public string Material { get; set; }

            [Name("Unit Cost")]
            public double CostPerKilo { get; set; }

            [Name("Barstock ID")]
            public string BarId { get; set; }

            [Name("Bar Length (mm)")]
            public int BarLength { get; set; }

            [Name("Bar Cost")]
            public double? BarCost { get; set; }

            [Name("Item Length Consumption")]
            public double MaterialBudget { get; set; }

            [Name("Material Cost")]
            public double MaterialCost { get; set; } = 0;

            [Name("Time Cost")]
            public double TimeCost { get; set; }

            [Name("Total Cost")]
            public double TotalCost { get; set; }

            public string TimeModel { get; set; }
            public bool HasBeenProduced { get; set; }

            public object Clone()
            {
                string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ItemCost>(serialised);
            }
        }
    }
}
