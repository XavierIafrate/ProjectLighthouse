using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml.Packaging;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.View.Core;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ViewModel.Commands.Administration;
using static ProjectLighthouse.Model.BaseObject;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class DatabaseManagerViewModel : BaseViewModel
    {
        public List<LatheManufactureOrder> Orders { get; set; }

        public GetRecordsAsCsvCommand GetRecordsAsCsvCmd { get; set; }

        public DatabaseManagerViewModel()
        {
            GetRecordsAsCsvCmd = new(this);

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();

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
                case "ProductImport":
                    GetTurnedProductImportSheet();
                    break;
                case "Wizard":
                    ImportWizard window = new() { Owner = App.MainViewModel.MainWindow };
                    window.ShowDialog();
                    break;
                default:
                    throw new NotImplementedException();
            };
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
                DateTime date = DateTime.Today.AddDays(i*-1);

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

        class WorkloadDay
        {
            public DateTime Day { get; set; }
            public string WeekId { get; set; }
            public int CountOfOrders { get; set; }
            public int CountOfProductionOrders { get;  set; }   
            public int CountOfDevelopmentOrders { get; set; }
            public double AverageTurnaroundTime { get; set; }
        }

        private void GetTurnedProductImportSheet()
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

        private void GenerateItemCostingSheet()
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
                    .OrderBy(x=> x.Size)
                    .ToList();

                if (candidates.Count == 0)
                {
                    continue;
                }

                BarStock bar = candidates.First();


                ItemCost newItem = new()
                {
                    Product = string.IsNullOrEmpty(product.ExportProductName) ? product.ProductName : product.ExportProductName,
                    CycleTime = product.CycleTime,
                    BarId = bar.Id,
                    Material = material.MaterialCode,
                    BarLength = bar.Length,
                    BarCost = (double)bar.Cost / 100,
                    ProductMajorLength = product.MajorLength + product.PartOffLength + 2,
                };

                itemCosts.Add(newItem);
            }

            CSVHelper.WriteListToCSV(itemCosts, "ItemMaterialDetails");

        }


        public class ItemCost
        {
            [Name("Item ID")]
            public string Product { get; set; }
            
            [Name("Cycle Time (seconds)")]
            public int CycleTime { get; set; }
            
            [Name("Material ID")]
            public string Material { get; set; }
            
            [Name("Barstock ID")]
            public string BarId { get; set; }
            
            [Name("Bar Length (mm)")]
            public int BarLength { get; set; }
            
            [Name("Bar Cost")]
            public double BarCost { get; set; }
            
            [Name("Item Major Length (mm)")]
            public double ProductMajorLength { get; set; }
        }
    }
}
