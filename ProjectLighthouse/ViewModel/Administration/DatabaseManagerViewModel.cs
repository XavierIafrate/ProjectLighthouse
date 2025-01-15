using CsvHelper.Configuration.Attributes;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class DatabaseManagerViewModel : BaseViewModel
    {
        public ManageUsersViewModel ManageUsersViewModel { get; set; }
        public MachineViewModel LatheViewModel { get; set; }
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
                case "ItemCosting":
                    GenerateItemCostingSheet();
                    break;

                case "Attachment":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Attachment>(), type.Replace(' ', '_'));
                    break;

                case "Bar Issue":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<BarIssue>(), type.Replace(' ', '_'));
                    break;

                case "Bar Stock":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<BarStock>(), type.Replace(' ', '_'));
                    break;

                case "Bar Stock Purchase":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<BarStockPurchase>(), type.Replace(' ', '_'));
                    break;

                case "Breakdown Code":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<BreakdownCode>(), type.Replace(' ', '_'));
                    break;

                case "Calibrated Equipment":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<CalibratedEquipment>(), type.Replace(' ', '_'));
                    break;

                case "Calibration Certificate":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<CalibrationCertificate>(), type.Replace(' ', '_'));
                    break;

                case "Delivery Item":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<DeliveryItem>(), type.Replace(' ', '_'));
                    break;

                case "Delivery Note":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<DeliveryNote>(), type.Replace(' ', '_'));
                    break;

                case "General Manufacture Order":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<GeneralManufactureOrder>(), type.Replace(' ', '_'));
                    break;

                case "Lathe":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Lathe>(), type.Replace(' ', '_'));
                    break;

                case "Lathe Manufacture Order":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<LatheManufactureOrder>(), type.Replace(' ', '_'));
                    break;

                case "Lathe Manufacture Order Item":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<LatheManufactureOrderItem>(), type.Replace(' ', '_'));
                    break;

                case "Login":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Login>(), type.Replace(' ', '_'));
                    break;

                case "Lot":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Lot>(), type.Replace(' ', '_'));
                    break;

                case "Machine":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Machine>(), type.Replace(' ', '_'));
                    break;

                case "Machine Breakdown":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<MachineBreakdown>(), type.Replace(' ', '_'));
                    break;

                case "Machine Service":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<MachineService>(), type.Replace(' ', '_'));
                    break;

                case "Maintenance Event":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<MaintenanceEvent>(), type.Replace(' ', '_'));
                    break;

                case "Material Info":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<MaterialInfo>(), type.Replace(' ', '_'));
                    break;

                case "NC Program":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<NcProgram>(), type.Replace(' ', '_'));
                    break;

                case "NC Program Commit":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<NcProgramCommit>(), type.Replace(' ', '_'));
                    break;

                case "Non Turned Item":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<NonTurnedItem>(), type.Replace(' ', '_'));
                    break;

                case "Note":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Note>(), type.Replace(' ', '_'));
                    break;

                case "Order Drawing":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<OrderDrawing>(), type.Replace(' ', '_'));
                    break;

                case "Permission":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Permission>(), type.Replace(' ', '_'));
                    break;

                case "Product":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Product>(), type.Replace(' ', '_'));
                    break;

                case "Product Group":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<ProductGroup>(), type.Replace(' ', '_'));
                    break;

                case "Request":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Request>(), type.Replace(' ', '_'));
                    break;

                case "Request Item":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<RequestItem>(), type.Replace(' ', '_'));
                    break;

                case "Standard":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<Standard>(), type.Replace(' ', '_'));
                    break;

                case "Technical Drawing":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<TechnicalDrawing>(), type.Replace(' ', '_'));
                    break;

                case "Tolerance Definition":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<ToleranceDefinition>(), type.Replace(' ', '_'));
                    break;

                case "Turned Product":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<TurnedProduct>(), type.Replace(' ', '_'));
                    break;

                case "User":
                    CSVHelper.WriteListToCSV(DatabaseHelper.Read<User>(), type.Replace(' ', '_'));
                    break;

                case "SpeedTest":
                    RunSpeedTest();
                    break;

                default:
                    return;
            }
        }

        private static void RunSpeedTest()
        {
            TimeSpan networkReadTime;
            TimeSpan sisterNetworkReadTime;
            TimeSpan localReadTime;
            TimeSpan newServerReadTime;
            TimeSpan gbchOperaT01ReadTime;
            TimeSpan groupdbReadTime;
            TimeSpan goodsyncReadTime;

            try
            {
                //File.Copy(DatabaseHelper.DatabasePath, @"C:\Temp\test.db3", overwrite:true);
                //File.Copy(DatabaseHelper.DatabasePath, @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\test.db3", overwrite:true);
                //File.Copy(DatabaseHelper.DatabasePath, @"\\GBCH-LIGHT-01\Lighthouse\test.db3", overwrite:true);
                //File.Copy(DatabaseHelper.DatabasePath, @"\\GBCH-OPERA-T01\TEST\test.db3", overwrite:true);
                ////File.Copy(DatabaseHelper.DatabasePath, @"\\goodsync01\WMS\Backups\test.db3", overwrite:true);
                //File.Copy(DatabaseHelper.DatabasePath, @"\\GBCH-LIGHT-01\Lighthouse\test.db3", overwrite:true);
                //File.Copy(DatabaseHelper.DatabasePath, @"\\groupdb01\WMS\xav_test\test.db3", overwrite:true);

                var conn = new SQLiteConnection(DatabaseHelper.DatabasePath);
                networkReadTime = RunSpeedTest(conn);

                conn = new SQLiteConnection(@"C:\Temp\test.db3");
                localReadTime = RunSpeedTest(conn);

                conn = new SQLiteConnection(@"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\test.db3");
                sisterNetworkReadTime = RunSpeedTest(conn);

                //conn = new SQLiteConnection(@"\\groupdb01\WMS\xav_test\test.db3");
                //groupdbReadTime = RunSpeedTest(conn);

                //conn = new SQLiteConnection(@"\\GBCH-LIGHT-01\Lighthouse\test.db3");
                //newServerReadTime = RunSpeedTest(conn);

                //conn = new SQLiteConnection(@"\\GBCH-OPERA-T01\TEST\test.db3");
                //gbchOperaT01ReadTime = RunSpeedTest(conn);

                //conn = new SQLiteConnection(@"\\goodsync01\WMS\Backups\test.db3");
                //goodsyncReadTime = RunSpeedTest(conn);
            }
            catch (Exception e)
            {
                NotificationManager.NotifyHandledException(e);
                return;
            }

            MessageBox.Show($"Network [{DatabaseHelper.DatabasePath}] : {networkReadTime.TotalSeconds:0.000}s{Environment.NewLine}" +
                $@"Adjacent File [\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\test.db3] : {sisterNetworkReadTime.TotalSeconds:0.000}s{Environment.NewLine}" +
                $@"Local [C:\Temp\test.db3] : {localReadTime.TotalSeconds:0.000}s{Environment.NewLine}");
            //+
            //$@"Opera Test Server [\\GBCH-OPERA-T01\TEST\test.db3] : {gbchOperaT01ReadTime.TotalSeconds:0.000}s{Environment.NewLine}" +
            //$@"GroupDB [\\groupdb01\WMS\xav_test\test.db3] : {groupdbReadTime.TotalSeconds:0.000}s{Environment.NewLine}" +
            ////$@"GoodSync [\\goodsync01\WMS\Backups\test.db3] : {goodsyncReadTime.TotalSeconds:0.000}s{Environment.NewLine}" +
            //$@"Network [\\GBCH-LIGHT-01\Lighthouse\test.db3] : {newServerReadTime.TotalSeconds:0.000}s", "Results");
        }

        private static TimeSpan RunSpeedTest(SQLiteConnection conn)
        {
            List<Note> result;
            DateTime startTime = DateTime.Now;
            using (conn)
            {
                conn.CreateTable<Note>();
                result = conn.Table<Note>().ToList();
            }

            DateTime endTime = DateTime.Now;

            return (endTime - startTime);
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

            Dictionary<string, TimeModel> cachedModels = new();

            List<ItemCost> itemCosts = new();
            foreach (TurnedProduct sku in products)
            {
                ProductGroup? memberOf = groups.Find(x => x.Id == sku.GroupId);
                MaterialInfo? material = materials.Find(x => x.Id == sku.MaterialId);


                if (memberOf is null || material is null)
                {
                    continue;
                }


                List<BarStock> candidates = barStock.Where(x =>
                        x.Size >= (memberOf.MinBarSize ?? memberOf.MajorDiameter)
                        && x.IsHexagon == memberOf.UsesHexagonBar
                        && x.MaterialId == material.Id)
                    .OrderBy(x => x.Size)
                    .ToList();

                if (candidates.Count == 0)
                {
                    continue;
                }

                TimeModel timeModel = GetTimeModel(sku, products, memberOf, material, cachedModels);


                BarStock bar = candidates.First();

                ItemCost newItem = new(sku, bar, material, App.Constants.AbsorptionRate, timeModel, 2);

                itemCosts.Add(newItem);
            }

            CSVHelper.WriteListToCSV(itemCosts, "ItemMaterialDetails");
        }

        private static TimeModel GetTimeModel(TurnedProduct sku, List<TurnedProduct> products, ProductGroup group, MaterialInfo material, Dictionary<string, TimeModel> cachedModels)
        {
            if (cachedModels.ContainsKey($"{sku.GroupId}>{material.Id}"))
            {
                return cachedModels[$"{sku.GroupId}>{material.Id}"];
            }


            TimeModel timeModel;
            List<TurnedProduct> timeResponseGroup = products.Where(x => x.GroupId == group.Id && x.MaterialId == material.Id && !x.IsSpecialPart && !x.Retired && x.CycleTime > 0).ToList();
            if (timeResponseGroup.Count == 0)
            {
                timeModel = TimeModel.Default(sku.MajorDiameter);
            }
            else
            {
                try
                {
                    timeModel = OrderResourceHelper.GetCycleResponse(timeResponseGroup);
                }
                catch
                {
                    timeModel = TimeModel.Default(sku.MajorDiameter);
                }
            }

            if (sku.GroupId is int groupId)
            {
                cachedModels.Add($"{sku.GroupId}>{material.Id}", timeModel);
            }

            return timeModel;
        }

        public class ItemCost
        {
            public ItemCost(TurnedProduct product, BarStock bar, MaterialInfo materialInfo, double absorptionRate, TimeModel model, double partOff)
            {
                Product = product.ProductName;
                CycleTime = model.At(product.MajorLength);
                TimeCost = CycleTime * absorptionRate;

                Material = materialInfo.MaterialCode;
                CostPerKilo = (double)(materialInfo.Cost ?? 0) / 100;
                BarId = bar.Id;
                bar.MaterialData = materialInfo;
                BarMass = bar.GetUnitMassOfBar();
                BarCost = (bar.ExpectedCost ?? 0) / 100;
                MaterialBudget = product.MajorLength + product.PartOffLength + partOff;

                MaterialCost = 1.1 * BarCost * (MaterialBudget / (bar.Length - App.Constants.BarRemainder));

                TotalCost = TimeCost + MaterialCost;

                TimeModel = model.ToString();

                HasBeenProduced = product.CycleTime > 0;
            }


            [Name("SKU")]
            public string Product { get; set; }

            [Name("Cycle Time [s]")]
            public int CycleTime { get; set; }

            [Name("Material")]
            public string Material { get; set; }

            [Name("Barstock ID")]
            public string BarId { get; set; }

            [Name("Material Cost [GBP/kg]")]
            public double CostPerKilo { get; set; }

            [Name("Bar Mass [kg]")]
            public double BarMass { get; set; }

            [Name("Expected Bar Cost [GBP]")]
            public double BarCost { get; set; }

            [Name("SKU Bar Consumption [mm]")]
            public double MaterialBudget { get; set; }

            [Name("SKU Material Cost [GBP]")]
            public double MaterialCost { get; set; } = 0;

            [Name("SKU Time Cost [GBP]")]
            public double TimeCost { get; set; }

            [Name("SKU Total Cost [GBP]")]
            public double TotalCost { get; set; }

            [Name("Time Model Code")]
            public string TimeModel { get; set; }

            [Name("SKU Produced")]
            public bool HasBeenProduced { get; set; }

            public object Clone()
            {
                string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ItemCost>(serialised);
            }
        }
    }
}
