using CsvHelper.Configuration.Attributes;
using PdfSharp.Pdf;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using ViewModel.Commands.Administration;
using static ProjectLighthouse.Model.BaseObject;

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
                case "WorkloadOrders":
                    GetWorkload();
                    break;
                case "ItemCosting":
                    GenerateItemCostingSheet();
                    break;
                case "BarStockEstimate":
                    GetBarInventory();
                    break;
                case "ProductImport":
                    GetTurnedProductImportSheet();
                    break;
                case "Wizard":
                    TestHTMLReport();
                    //AddTimeCodes();
                    //ImportWizard window = new() { Owner = App.MainViewModel.MainWindow };
                    //window.ShowDialog();
                    break;
                case "Delivered":
                    GetDeliveredItems();
                    break;
                case "TimeModels":
                    GetTimeModels();
                    break;
                case "Drawings":

                    List<TechnicalDrawing> drawings = DatabaseHelper.Read<TechnicalDrawing>();
                    CSVHelper.WriteListToCSV(drawings, "drawings");
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

        private static void GetTimeModels()
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>().Where(x => !x.Retired && !x.IsSpecialPart && x.GroupId != null).ToList();
            List<ProductGroup> groups = DatabaseHelper.Read<ProductGroup>().Where(x => x.ProductId != 65 && x.Status == ProductGroup.GroupStatus.Active).ToList();
            List<MaterialInfo> materials = DatabaseHelper.Read<MaterialInfo>();
            List<BarStock> barStock = DatabaseHelper.Read<BarStock>().Where(x => !x.IsDormant).ToList();

            List<Output> result = new();

            foreach (ProductGroup group in groups)
            {
                List<TurnedProduct> productsInGroup = products.Where(x => x.GroupId == group.Id && x.MaterialId is not null).ToList();
                List<int?> materialIds = productsInGroup.Select(x => x.MaterialId).OrderBy(x => x).Distinct().ToList();
                foreach (int? id in materialIds)
                {
                    if (id is null) continue;

                    BarStock? bar = group.GetRequiredBarStock(barStock, (int)id);
                    MaterialInfo? m = materials.Find(m => m.Id == id);

                    if (m is null)
                    {
                        continue;
                    }

                    List<TurnedProduct> productsInGroupWithMaterial = productsInGroup.Where(x => x.MaterialId == id).ToList();
                    TimeModel model;

                    try
                    {
                        model = OrderResourceHelper.GetCycleResponse(productsInGroupWithMaterial);
                    }
                    catch
                    {
                        result.Add(new() { Group = group.Name, Material = m.MaterialCode, BaseRecordCount = productsInGroupWithMaterial.Count }); ;
                        continue;
                    }

                    if (group.Name == "P0154.060" && id == 1)
                    {
                        List<Price> prices = new();
                        foreach (TurnedProduct p in productsInGroupWithMaterial)
                        {
                            double price = GetPrice(p, bar, m, 0.00505, model);
                            prices.Add(new() { SKU = p.ProductName, SKUPrice = $"£{price:0.000}" });
                        }
                        CSVHelper.WriteListToCSV(prices, "prices");
                    }

                    result.Add(new() { Group = group.Name, Material = m.MaterialCode, BaseRecordCount = productsInGroupWithMaterial.Count, TimeModel = model.ToString(), Gradient = model.Gradient, Intercept = model.Intercept, Confidence = model.CoefficientOfDetermination, Floor = model.Floor, NumRecords = model.RecordCount });


                }
            }
            CSVHelper.WriteListToCSV(result, "timeModels");

        }

        public class Price
        {
            public string SKU { get; set; }
            public string SKUPrice { get; set; }
        }

        public class Output
        {
            public string Group { get; set; }
            public string Material { get; set; }
            public int BaseRecordCount { get; set; }
            public string TimeModel { get; set; }
            public double? Gradient { get; set; }
            public double? Intercept { get; set; }
            public int? Floor { get; set; }
            public int? NumRecords { get; set; }
            public double? Confidence { get; set; }
        }


        private static void TestHTMLReport()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string htmlString = "<h1>Costing Analysis</h1> <p>This is an HTML document which is converted to a pdf file.</p>";
            PdfDocument pdfDocument = PdfGenerator.GeneratePdf(htmlString, PdfSharp.PageSize.A4);
            pdfDocument.Save(@"C:\Users\x.iafrate\Dev\test.pdf");
        }

        private static void AddTimeCodes()
        {
            List<LatheManufactureOrder> activeOrders = DatabaseHelper.Read<LatheManufactureOrder>().Where(x => x.State < OrderState.Running && string.IsNullOrEmpty(x.TimeCodePlanned)).ToList();
            List<TurnedProduct> turnedProducts = DatabaseHelper.Read<TurnedProduct>().Where(x => !x.Retired).ToList();
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>();

            foreach (LatheManufactureOrder order in activeOrders)
            {
                List<TurnedProduct> dataPool = turnedProducts.Where(x => x.GroupId == order.GroupId && x.MaterialId == order.MaterialId).ToList();

                TimeModel timeModel;

                try
                {
                    timeModel = OrderResourceHelper.GetCycleResponse(dataPool);
                }
                catch
                {
                    timeModel = TimeModel.Default(order.MajorDiameter);
                }
                order.TimeCodeIsEstimate = timeModel.CoefficientOfDetermination < 0.5;
                order.TimeModelPlanned = timeModel;

                List<LatheManufactureOrderItem> orderItems = items.Where(x => x.AssignedMO == order.Name).ToList();
                foreach (LatheManufactureOrderItem item in orderItems)
                {
                    TurnedProduct product = turnedProducts.Find(x => x.ProductName == item.ProductName);
                    if (item.PreviousCycleTime is not null) continue;

                    if (product!.CycleTime == 0 && item.PreviousCycleTime is null)
                    {
                        item.ModelledCycleTime = timeModel.At(item.MajorLength);
                    }
                    else
                    {
                        item.PreviousCycleTime = product.CycleTime;
                    }

                    DatabaseHelper.Update(item);
                }

                order.TimeToComplete = OrderResourceHelper.CalculateOrderRuntime(order, orderItems);
                DatabaseHelper.Update(order);
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

        private void GetWorkload()
        {
            List<WorkloadDay> workload = new();

            List<LatheManufactureOrder> orders = Orders
                                                    .Where(x =>
                                                        x.State < OrderState.Cancelled
                                                        && x.StartDate.Date.AddMonths(18) > DateTime.Now
                                                        && !string.IsNullOrEmpty(x.AllocatedMachine))
                                                    .OrderBy(x => x.StartDate)
                                                    .ToList();

            for (int i = 0; i < 365; i++)
            {
                DateTime date = DateTime.Today.AddDays(i * -1);

                WorkloadDay data = new()
                {
                    Day = date,
                    WeekId = $"W{ISOWeek.GetWeekOfYear(date):00}-{date:yyyy}"
                };

                List<LatheManufactureOrder> ordersAtTime = orders.Where(x => x.CreatedAt <= date && x.EndsAt() >= date).ToList();

                data.CountOfOrders = ordersAtTime.Count;
                data.CountOfProductionOrders = ordersAtTime.Where(x => !x.IsResearch).Count();
                data.CountOfDevelopmentOrders = ordersAtTime.Where(x => x.IsResearch).Count();

                TimeSpan totalWorkTime = new();
                ordersAtTime
                    .Where(x => !x.IsResearch)
                    .ToList()
                    .ForEach(x => totalWorkTime += x.EndsAt() - x.CreatedAt);

                data.AverageTurnaroundTime = totalWorkTime.TotalDays / 7;
                data.AverageTurnaroundTime /= ordersAtTime.Where(x => !x.IsResearch).Count();

                workload.Add(data);
            }

            workload.Reverse();

            CSVHelper.WriteListToCSV(workload, "workloadOrders");
        }

        private static double GetPrice(TurnedProduct product, BarStock barStock, MaterialInfo materialInfo, double absorptionRate, TimeModel model)
        {
            if (materialInfo.Cost is null) return 0;

            barStock.MaterialData = materialInfo;
            double unitMass = barStock.GetUnitMassOfBar();
            double massForProduct = unitMass * (product.MajorLength + product.PartOffLength + 2) / 2700;
            double costOfMaterial = ((double)materialInfo.Cost / 100) * massForProduct;


            double costOfTime = absorptionRate * model.At(product.MajorLength);

            return costOfMaterial + costOfTime;
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

        class WorkloadDay
        {
            public DateTime Day { get; set; }
            public string WeekId { get; set; }
            public int CountOfOrders { get; set; }
            public int CountOfProductionOrders { get; set; }
            public int CountOfDevelopmentOrders { get; set; }
            public double AverageTurnaroundTime { get; set; }
        }

        private static void GetTurnedProductImportSheet()
        {
            PropertyInfo[] properties = typeof(TurnedProduct).GetProperties();
            List<string> propertiesForExport = new();


            foreach (PropertyInfo property in properties)
            {
                Import? importAttribute = property.GetCustomAttribute<Import>();
                if (importAttribute is null)
                {
                    continue;
                }

                propertiesForExport.Add(importAttribute.Name);
            }

            string headerLine = string.Join(',', propertiesForExport);

            try
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\template.csv", headerLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

                //if (newItem.Product.EndsWith("2X"))
                //{
                //    ItemCost tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "A2");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "VI");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "TX");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "2S");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "2V");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "2E");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "2N");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("2X", "2F");
                //    itemCosts.Add(tmpItem);


                //}
                //else if (newItem.Product.EndsWith("4X"))
                //{
                //    ItemCost tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "A4");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "4S");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "4V");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "4E");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "4N");
                //    itemCosts.Add(tmpItem);

                //    tmpItem = (ItemCost)newItem.Clone();
                //    tmpItem.Product = newItem.Product.Replace("4X", "4F");
                //    itemCosts.Add(tmpItem);
                //}
                //else
                //{
                //    itemCosts.Add(newItem);
                //}
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
