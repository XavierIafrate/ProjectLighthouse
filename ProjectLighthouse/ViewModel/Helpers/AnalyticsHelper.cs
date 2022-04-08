using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class AnalyticsHelper
    {
        private static DataStore Data { get; set; }
        public static HeroStats Hero { get; set; }
        public static PartsMadeAllTimeGraph PartsMadeAllTime { get; set; }
        public static OperatingPerformance Performance { get; set; }
        public AnalyticsHelper()
        {
            Initialise();
        }

        public static void Initialise()
        {
            Data = new();
            Hero = new();
            PartsMadeAllTime = new();
            Performance = new();
        }

        public class DataStore
        {
            public List<Lathe> Lathes { get; set; }
            public List<TurnedProduct> TurnedProducts { get; set; }
            public List<LatheManufactureOrder> Orders { get; set; }
            public List<LatheManufactureOrderItem> OrderItems { get; set; }
            public List<MachineService> Services { get; set; }
            public List<ResearchTime> ResearchDevelopment { get; set; }
            public List<ScheduleItem> ScheduleItems { get; set; }
            public List<Lot> Lots { get; set; }
            public List<Request> Requests { get; set; }
            public List<MachineOperatingBlock> MachinePerformance { get; set; }
            public List<MachineOperatingBlock> MachinePerformanceByDay { get; set; }

            public DataStore()
            {
                Lathes = DatabaseHelper.Read<Lathe>();
                TurnedProducts = DatabaseHelper.Read<TurnedProduct>();
                Orders = DatabaseHelper.Read<LatheManufactureOrder>();
                OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();
                Services = DatabaseHelper.Read<MachineService>();
                ResearchDevelopment = DatabaseHelper.Read<ResearchTime>();
                Lots = DatabaseHelper.Read<Lot>();
                Requests = DatabaseHelper.Read<Request>();
                MachinePerformance = DatabaseHelper.Read<MachineOperatingBlock>().OrderBy(x => x.StateEntered).ToList();

                MachinePerformance.AddRange(GetLatheCurrentStates());

                MachinePerformanceByDay = MachinePerformanceHelper.SplitBlocksIntoDays(MachinePerformance);

                MakeOrdersComplete();
                PopulateScheduleItems();
            }

            private List<MachineOperatingBlock> GetLatheCurrentStates()
            {
                List<MachineStatistics> lastKnownStates = new();
                lastKnownStates = MachineStatsHelper.GetStats() ?? new();
                List<MachineOperatingBlock> incompleteBlocks = new();

                DateTime now = DateTime.Now;
                foreach (MachineStatistics state in lastKnownStates)
                {
                    MachineOperatingBlock lastCompleteBlock = MachinePerformance.Last(s => s.MachineID == state.MachineID);
                    incompleteBlocks.Add(new()
                    {
                        MachineID = state.MachineID,
                        MachineName = state.MachineName,
                        State = state.Status,
                        StateEntered = lastCompleteBlock.StateLeft,
                        StateLeft = now,
                        SecondsElapsed = (now - lastCompleteBlock.StateLeft).TotalSeconds,
                        CycleTime = state.CycleTime,
                    });
                }

                return incompleteBlocks;
            }


            private void PopulateScheduleItems()
            {
                ScheduleItems = new();
                ScheduleItems.AddRange(Orders);
                ScheduleItems.AddRange(ResearchDevelopment);
                ScheduleItems.AddRange(Services);
            }

            private void MakeOrdersComplete()
            {
                for (int i = 0; i < Orders.Count; i++)
                {
                    Orders[i].OrderItems = OrderItems.Where(x => x.AssignedMO == Orders[i].Name).ToList();
                }
            }
        }

        public class HeroStats
        {
            public int TotalPartsMade { get; set; }
            public int TotalPartsMadeThisYear { get; set; }
            public int NumberOfOrders { get; set; }

            public HeroStats()
            {
                TotalPartsMade = Data.Lots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
                TotalPartsMadeThisYear = Data.Lots.Where(x => x.IsDelivered && x.Date.Year == DateTime.Now.Year)
                    .Sum(x => x.Quantity);
                NumberOfOrders = Data.Orders.Count;
            }
        }

        public class OperatingPerformance
        {
            public List<DayPerformance> Performance { get; set; }
            public class DayPerformance
            {
                public DayPerformance(DateTime date)
                {
                    Date = date;
                    LatheData = new();
                    for (int i = 0; i < Data.Lathes.Count; i++)
                    {
                        LatheData.Add(new(Data.Lathes[i], Date));
                    }

                    OverallRuntimeByLatheModel = new();
                    List<string> models = Data.Lathes.Select(x => x.Model).Distinct().ToList();

                    for (int i = 0; i < models.Count; i++)
                    {
                        OverallRuntimeByLatheModel.Add( new(
                            models[i],
                            LatheData.Where(x => x.Lathe.Model == models[i])
                                     .Average(x => x.OperatingData.Running)
                            ));
                    }
                }

                public DateTime Date { get; set; }
                public List<DailyLathePerformance> LatheData { get; set; }
                public List<RuntimeAggregation> OverallRuntimeByLatheModel { get; set; }

                public class RuntimeAggregation
                { 
                    public string Model { get; set; }   
                    public double AverageRuntime { get; set; }
                    public RuntimeAggregation(string model, double avg)
                    {
                        Model = model;
                        AverageRuntime = avg;
                    }
                }

            public class DailyLathePerformance
                {
                    public DailyLathePerformance(Lathe lathe, DateTime date)
                    {
                        Lathe = lathe;
                        Lots = Data.Lots.Where(x => x.DateProduced.Date == date && x.FromMachine == Lathe.Id).ToList();
                        Orders = Data.ScheduleItems.Where(x => x.StartDate.Date == date && x.AllocatedMachine == Lathe.Id).ToList();
                        MachineOperatingBlocks = Data.MachinePerformanceByDay.Where(x => x.StateEntered.Date == date && x.MachineID == lathe.Id).ToList();
                        OperatingData = new(MachineOperatingBlocks);

                        TotalScrap = Lots.Where(x => x.IsReject).Sum(x => x.Quantity);
                        TotalGood = Lots.Where(x => x.IsAccepted).Sum(x => x.Quantity);
                        TotalQuarantined = Lots.Where(x => !x.IsReject && !x.IsAccepted).Sum(x => x.Quantity);
                        TotalRecorded = TotalGood + TotalQuarantined + TotalScrap;
                        ProjectedQuantity = MachineOperatingBlocks.Sum(x => x.GetCalculatedPartsProduced());
                    }

                    public int TotalScrap { get; set; }
                    public int TotalGood { get; set; }
                    public int TotalQuarantined { get; set; }
                    public int TotalRecorded { get; set; }
                    public int ProjectedQuantity { get; set; }

                    public Lathe Lathe { get; set; }
                    public List<ScheduleItem> Orders { get; set; }
                    public List<Lot> Lots { get; set; }
                    public List<MachineOperatingBlock> MachineOperatingBlocks { get; set; }
                    public OperatingInfo OperatingData { get; set; }
                    public class OperatingInfo
                    {
                        public OperatingInfo(List<MachineOperatingBlock> raw)
                        {
                            //RawData = MachineStatsHelper.Convolute(raw);
                            RawData = raw;
                            MajorSegments = RawData.Where(x => x.SecondsElapsed > 1200).ToList();
                            Running = (double)RawData.Where(x => x.State == nameof(Running)).Sum(x => x.SecondsElapsed)/(double)864;
                            Setting = (double)RawData.Where(x => x.State == nameof(Setting)).Sum(x => x.SecondsElapsed)/(double)864;
                            Breakdown = (double)RawData.Where(x => x.State == nameof(Breakdown)).Sum(x => x.SecondsElapsed)/(double)864;
                            Offline = (double)RawData.Where(x => x.State == nameof(Offline)).Sum(x => x.SecondsElapsed)/(double)864;
                            Idle = (double)RawData.Where(x => x.State == nameof(Idle)).Sum(x => x.SecondsElapsed)/(double)864;
                        }

                        public List<MachineOperatingBlock> RawData { get; set; }
                        public List<MachineOperatingBlock> MajorSegments { get; set; }
                        public double Running { get; set; }
                        public double Setting { get; set; }
                        public double Breakdown { get; set; }
                        public double Offline { get; set; }    
                        public double Idle { get; set; }   
                    }
                }
            }

            public OperatingPerformance()
            {
                GetPerformanceData();
            }

            private void GetPerformanceData()
            {
                Performance = new();
                for (int i = -1; i >= -7; i--)
                {
                    Performance.Add(new(DateTime.Today.AddDays(i)));
                }

            }
        }

        public class PartsMadeAllTimeGraph : Graph
        {

            public Func<DateTime, string> X_Formatter { get; set; }
            public Func<int, string> Y_Formatter { get; set; }

            public PartsMadeAllTimeGraph()
            {
                Title = "Parts Made - All Time";
                X_Formatter = value => $"{value}";
                Y_Formatter = value => $"{value:#,##0}";
                GetChartData();
            }

            private void GetChartData()
            {
                Series = new();
                List<IGrouping<DateTime, Lot>> chartLots = Data.Lots
                    .Where(x => x.IsDelivered)
                    .OrderBy(x => x.Date)
                    .GroupBy(x => x.Date.Date)
                    .ToList();

                ChartValues<DateTimePoint> chartData = new();
                int total = 0;
                for (int i = 0; i < chartLots.Count; i++)
                {
                    total += chartLots[i].Sum(x => x.Quantity);
                    chartData.Add(new(chartLots[i].Key, total));
                }

                Series.Add(new LineSeries()
                {
                    Title = "Sum of Parts Made",
                    LineSmoothness = 0,
                    PointGeometrySize = 0,
                    Values = chartData
                });
            }
        }

        public class Graph
        {
            public SeriesCollection Series { get; set; }
            public string Title { get; set; }
        }
    }
}
