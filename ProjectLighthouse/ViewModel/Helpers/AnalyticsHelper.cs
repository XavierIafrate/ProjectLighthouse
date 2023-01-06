using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using Axis = LiveChartsCore.SkiaSharpView.Axis;

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

                MachinePerformance.AddRange(GetLatheCurrentStates(MachinePerformance));

                MachinePerformanceByDay = MachinePerformanceHelper.SplitBlocksIntoDays(MachinePerformance, 6);

                MakeOrdersComplete();
                PopulateScheduleItems();
            }

            public List<MachineOperatingBlock> GetLatheCurrentStates(List<MachineOperatingBlock> data)
            {
                List<MachineStatistics> lastKnownStates = new();
                lastKnownStates = MachineStatsHelper.GetStats() ?? new();
                List<MachineOperatingBlock> incompleteBlocks = new();

                DateTime now = DateTime.Now;
                foreach (MachineStatistics state in lastKnownStates)
                {
                    MachineOperatingBlock lastCompleteBlock = data.Last(s => s.MachineID == state.MachineID);
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
                    List<MachineOperatingBlock> machineOperatingBlocks = MachinePerformanceHelper.SplitBlocksIntoDays(Data.MachinePerformance, hour: date.Hour);
                    for (int i = 0; i < Data.Lathes.Count; i++)
                    {
                        LatheData.Add(new(Data.Lathes[i], Date, machineOperatingBlocks));
                    }

                    OverallRuntimeByLatheModel = new();
                    List<string> models = Data.Lathes.Select(x => x.Model).Distinct().ToList();

                    for (int i = 0; i < models.Count; i++)
                    {
                        OverallRuntimeByLatheModel.Add(new(
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
                    public DailyLathePerformance(Lathe lathe, DateTime date, List<MachineOperatingBlock> operatingData, int resolution = 30, bool debug = false)
                    {
                        Lathe = lathe;
                        Lots = Data.Lots.Where(x => x.DateProduced.Date == date.Date && x.FromMachine == Lathe.Id).ToList();
                        Orders = Data.ScheduleItems.Where(x => x.StartDate.Date == date.Date && x.AllocatedMachine == Lathe.Id).ToList();

                        if (debug) CSVHelper.WriteListToCSV(operatingData.Where(x => x.MachineID == lathe.Id).ToList(), "allLatheOperatingData");

                        List<MachineOperatingBlock> relevantOperatingData = operatingData.Where(x => DateWithinRange(x.StateEntered, date) && x.MachineID == lathe.Id).ToList();

                        if (debug) CSVHelper.WriteListToCSV(relevantOperatingData, "relevantOperatingData");

                        MachineOperatingBlocks = MachinePerformanceHelper.Backfill(relevantOperatingData, date.Hour);
                        MachineOperatingBlocks = MachinePerformanceHelper.Convolute(relevantOperatingData, resolutionMinutes: resolution);
                        //if (debug) CSVHelper.WriteListToCSV(MachineOperatingBlocks, "after_convolution");

                        OperatingData = new(MachineOperatingBlocks);
                        if (debug) CSVHelper.WriteListToCSV(MachinePerformanceHelper.Backfill(relevantOperatingData, date.Hour), "after_backfill");


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
                            RawData = raw;
                            MajorSegments = RawData.Where(x => x.SecondsElapsed >= 1200).ToList();
                            Running = (double)RawData.Where(x => x.State == nameof(Running)).Sum(x => x.SecondsElapsed) / (double)864;
                            Setting = (double)RawData.Where(x => x.State == nameof(Setting)).Sum(x => x.SecondsElapsed) / (double)864;
                            Breakdown = (double)RawData.Where(x => x.State == nameof(Breakdown)).Sum(x => x.SecondsElapsed) / (double)864;
                            Offline = (double)RawData.Where(x => x.State == nameof(Offline)).Sum(x => x.SecondsElapsed) / (double)864;
                            Idle = (double)RawData.Where(x => x.State == nameof(Idle)).Sum(x => x.SecondsElapsed) / (double)864;
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
                    Performance.Add(new(DateTime.Today.AddDays(i).AddHours(6)));
                }
            }
        }

        public class PartsMadeAllTimeGraph : Graph, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public Func<DateTime, string> X_Formatter { get; set; }
            public Func<int, string> Y_Formatter { get; set; }
            
            public Axis[] XAxes { get; }
            public List<string> YearsAvailable { get; set; }    

            public PartsMadeAllTimeGraph()
            {
                X_Formatter = value => $"{value}";
                Y_Formatter = value => $"{value:#,##0}";
                GetChartData();
            }

            private void GetChartData()
            {
                int cutoffYear = DateTime.Today.Year - 5;
                List<IGrouping<DateTime, Lot>> chartLots = Data.Lots
                    .Where(x => x.IsDelivered && x.Date.Year >= cutoffYear)
                    .OrderBy(x => x.Date)
                    .GroupBy(x => x.Date.Date)
                    .ToList();

                List<DateTimePoint> chartData = new();

                int total = Data.Lots
                    .Where(x => x.IsDelivered && x.Date.Year < cutoffYear)
                    .Sum(x => x.Quantity);

                YearsAvailable = chartLots
                    .Select(x => x.Key.Year.ToString("YYYY"))
                    .Distinct()
                    .ToList();

                for (int i = 0; i < chartLots.Count; i++)
                {
                    total += chartLots[i].Sum(x => x.Quantity);
                    chartData.Add(new(chartLots[i].Key, total));
                }

                Series = new ISeries[]
                {
                    new LineSeries<DateTimePoint>
                    {
                        Values = chartData,
                        Name = "Total Parts Made"
                    }
                };

                OnPropertyChanged(nameof(Series));
            }
        }

        public class Graph
        {
            public ISeries[] Series { get; set; }
            public string Title { get; set; }
        }

        public string GetDailyEmailMessage(DateTime startingDate)
        {
            EmailReportPerformance helper = new();

            return $"<html style='font-family: sans-serif; margin: 0;'>" +
                $"{helper.GetEmailHeader(startingDate)}" +
                $"{helper.GetEmailContent(startingDate)}" +
                $"{helper.GetEmailFooter()}" +
                $"</html>";
        }

        public class EmailReportPerformance
        {
            public string GetEmailHeader(DateTime date)
            {
                string greeting = DateTime.Now.Hour < 12 ? "morning" : "afternoon";
                return $"<header style='margin: 50px 0 0 5%;'><h1> Good {greeting}," +
                    $"</h1><p style='font-size: 16pt;'>Here is your daily machine runtime " +
                    $"update for {FormatDateForEmailSubject(date)}:</p><p style='font-size: " +
                    $"12pt; color: gray; font-style: italic; margin-top: 5px;'>The data shown " +
                    $"is an evaluation of the period {date:dd/MM} at {date:HHmm}h to " +
                    $"{date.AddDays(1):dd/MM} at {date.AddDays(1):HHmm}h</p></header>";
            }

            public string GetEmailContent(DateTime startingDate)
            {
                List<OperatingPerformance.DayPerformance.DailyLathePerformance> latheOverview = new();

                List<MachineOperatingBlock> data = DatabaseHelper.Read<MachineOperatingBlock>()
                    .Where(x => (x.StateEntered - DateTime.Now).TotalDays < 30)
                    .OrderBy(x => x.StateEntered)
                    .ToList();

                data.AddRange(Data.GetLatheCurrentStates(data));
                data = MachinePerformanceHelper.SplitBlocksIntoDays(new(Data.MachinePerformanceByDay), hour: startingDate.Hour);

                ////DEBUG ONLY
                //data = data.Where(x => x.StateEntered.AddDays(5) > DateTime.Now).ToList();
                //CSVHelper.WriteListToCSV(data, "split_blocks");

                string htmlContent = "<article style='margin-top: 30px;'><ul style='width: 60%; min-width: 700px; margin: auto;list-style: none;padding: 0;'>";

                for (int i = 0; i < Data.Lathes.Count; i++)
                {
                    latheOverview.Add(new(Data.Lathes[i], startingDate, data, resolution: 1)); //true
                }

                foreach (OperatingPerformance.DayPerformance.DailyLathePerformance lathe in latheOverview)
                {
                    htmlContent += GetEmailContentForLathe(lathe);
                }

                htmlContent += "</ul></article>";

                return htmlContent;
            }

            public string GetEmailContentForLathe(OperatingPerformance.DayPerformance.DailyLathePerformance data)
            {
                string result = "";

                // header
                result += GetLatheEmailContent_Header(data);
                // badges
                result += GetLatheEmailContent_Badges(data);
                // timeline
                result += GetLatheEmailContent_Timeline(data);
                //analyis
                result += GetLatheEmailContent_Analysis(data);

                result += "</li>";
                return result;
            }

            public string GetLatheEmailContent_Header(OperatingPerformance.DayPerformance.DailyLathePerformance data)
            {
                string highlightColour;

                if (data.OperatingData.Running > 80)
                {
                    highlightColour = "#009688";
                }
                else if (data.OperatingData.Running > 50)
                {
                    highlightColour = "#F57C00";
                }
                else
                {
                    highlightColour = "#b71c1c";
                }

                return $"<li style='border: solid {highlightColour} 4px; border-radius: 15px; padding: 20px; margin-top: 10px;'><h2 style='font-size: 26pt; margin: 0;margin-bottom: 10px;'>{data.Lathe.FullName}</h2>";
            }

            public string GetLatheEmailContent_Badges(OperatingPerformance.DayPerformance.DailyLathePerformance data)
            {
                return $"<table style='width: 90%; margin: auto; margin-top: 12px; padding: 0;'>" +
                    $"<tr style='font-size: 14pt; font-weight: bold; text-align: center;'>" +
                    $"<td style='margin: 0 20px;color: #009688;display: inline-block;background: #00968822;padding: 8px 20px; border: solid #009688 3px; border-radius: 10px;'>" +
                    $"<p style='opacity: 0.7;margin: 0;padding: 0;'>RUNNING</p>" +
                    $"<p style='font-size: 18pt;margin: 0;padding: 0;'>{data.OperatingData.Running:N1}%</p>" +
                    $"</td>" +
                    $"" +
                    $"<td style='margin: 0 20px;color: #1565C0;display: inline-block;background: #1565C022;padding: 8px 20px; border: solid #1565C0 3px; border-radius: 10px;'>" +
                    $"<p style='opacity: 0.7;padding: 0; margin: 0;'>SETTING</p>" +
                    $"<p style='font-size: 18pt;padding: 0; margin: 0;'>{data.OperatingData.Setting:N1}%</p>" +
                    $"</td>" +
                    $"" +
                    $"<td style='margin: 0 20px;color: #b71c1c;display: inline-block;background: #b71c1c22;padding: 8px 20px; border: solid #b71c1c 3px; border-radius: 10px;'>" +
                    $"<p style='opacity: 0.7;padding: 0; margin: 0;'>BREAKDOWN</p>" +
                    $"<p style='font-size: 18pt;padding: 0; margin: 0;'>{data.OperatingData.Breakdown:N1}%</p>" +
                    $"</td>" +
                    $"</tr></table>";
            }

            public string GetLatheEmailContent_Timeline(OperatingPerformance.DayPerformance.DailyLathePerformance data)
            {
                string timelineHtmlContent = "<h3>Timeline</h3><div style='width: 90%; margin: auto;'>" +
                    "<table style='width: 100%; font-family: monospace; '><tr>" +
                    "<td><p style='margin: 0; padding: 0;'>0600h</p></td>" +
                    "<td><p style='margin: 0; padding: 0;text-align: center;'>1800h</p></td>" +
                    "<td><p style='margin: 0; padding: 0;text-align: right;'>0600h</p></td>" +
                    "</tr></table><div style='padding: 0 2ex;'>" +
                    "<table style='background-color: none; width: 100%; height: 35px; border: solid #555 2px; border-radius: 5px;'><tr>";

                for (int i = 0; i < data.MachineOperatingBlocks.Count; i++)
                {
                    MachineOperatingBlock block = data.MachineOperatingBlocks[i];
                    string colour = "#000";
                    if (block.State == "Breakdown")
                    {
                        colour = "#b71c1c";
                    }
                    else if (block.State == "Setting")
                    {
                        colour = "#1565C0";
                    }
                    else if (block.State == "Running")
                    {
                        colour = "#009688";
                    }
                    double percentOfDay = block.SecondsElapsed / 864;
                    timelineHtmlContent += $"<td style='width: {percentOfDay:N2}%;background-color: {colour}; display: table-cell; border: none; border-radius: 2px;'></td>";
                }

                timelineHtmlContent += "</tr></table></div></div>";
                return timelineHtmlContent;
            }

            public string GetLatheEmailContent_Analysis(OperatingPerformance.DayPerformance.DailyLathePerformance data)
            {
                string initialResult = "<h3>Analysis</h3><div style='width: 90%; margin: auto;'><ul style='padding: 0;'>";
                string result = initialResult;
                string colGood = "#009688";
                string colWarn = "#F57C00";
                string colBad = "#b71c1c";


                if (data.OperatingData.Running > 99.5)
                {
                    result += GetBasicListItem($"{data.Lathe.FullName} had 100% runtime.", colGood);
                }
                else if (data.OperatingData.Running > 95)
                {
                    result += GetBasicListItem($"{data.Lathe.FullName} achieved over <strong>95%</strong> uptime.", colGood);

                }
                else if (data.OperatingData.Running > 90)
                {
                    result += GetBasicListItem($"{data.Lathe.FullName} achieved over 90% uptime.", colGood);

                }
                else if (data.OperatingData.Running > 80)
                {
                    result += GetBasicListItem($"{data.Lathe.FullName} ran with over the 80% target.", colGood);
                }

                if (data.OperatingData.Setting * 0.24 > 1.5)
                {
                    if (data.Orders.Count == 0)
                    {
                        List<MachineOperatingBlock> settingBlocks = data.MachineOperatingBlocks
                            .Where(x => x.State == "Setting" && x.SecondsElapsed > 3600)
                            .OrderByDescending(x => x.SecondsElapsed)
                            .ToList();

                        string msg = $"{data.OperatingData.Setting * 0.24:N1} hours were spent in manual mode when no order was scheduled to be set.";
                        if (settingBlocks.Count > 0)
                        {
                            MachineOperatingBlock largestBlock = settingBlocks.First();
                            msg += $" The main period of unplanned manual operation was from {largestBlock.StateEntered:HH:mm} to {largestBlock.StateLeft:HH:mm} ({TimeSpan.FromSeconds(largestBlock.SecondsElapsed).TotalHours:N1} hrs)";
                        }

                        result += GetBasicListItem(msg, data.OperatingData.Setting * 0.24 < 4 ? colWarn : colBad);
                    }
                    else
                    {
                        if (data.OperatingData.Setting * 0.24 <= 5)
                        {
                            result += GetBasicListItem($"{data.Orders.First().Name} was set.", colGood);
                        }
                        else if (data.OperatingData.Setting * 0.24 <= 8)
                        {
                            result += GetBasicListItem($"{data.Orders.First().Name} was set, taking an above average amount of time - {data.OperatingData.Setting * 0.24:N1} hours.", colWarn);
                        }
                        else
                        {
                            result += GetBasicListItem($"{data.Orders.First().Name} was set but it took an excessive amount of time ({data.OperatingData.Setting * 0.24:N0} hours).", colBad);
                        }
                    }
                }
                else if (data.Orders.Count > 0)
                {
                    result += GetBasicListItem($"{data.Orders.First().Name} was scheduled to be set, but the lathe did not spend very long under manual control.", colWarn);
                }

                List<MachineOperatingBlock> majorBreakdowns = data.MachineOperatingBlocks.Where(x => x.State == "Breakdown" && x.SecondsElapsed > 30 * 60).ToList();
                foreach (MachineOperatingBlock majorBreakdown in majorBreakdowns)
                {
                    result += GetErrorMessageListItem(majorBreakdown);
                }

                if (result == initialResult)
                {
                    if (data.MachineOperatingBlocks.Count == 0)
                    {
                        result += GetBasicListItem($"It appears there is no data to analyse...", colBad);
                    }
                    else if (data.MachineOperatingBlocks.Count == 1)
                    {
                        MachineOperatingBlock onlyData = data.MachineOperatingBlocks.First();
                        result += GetBasicListItem($"The only state recorded was from {onlyData.StateEntered:HH:mm} to {onlyData.StateLeft:HH:mm} in state '{onlyData.State}'", colWarn);
                    }
                    else
                    {
                        if (data.OperatingData.Idle > 50)
                        {
                            if (data.OperatingData.Idle > 95)
                            {
                                result += GetBasicListItem("The machine was idle all day.", colWarn);
                            }
                            else
                            {
                                result += GetBasicListItem("The machine was idle most of the day.", colWarn);
                            }
                        }
                        else if (data.OperatingData.Offline > 50)
                        {
                            if (data.OperatingData.Offline > 95)
                            {
                                result += GetBasicListItem("The machine was offline all day.", colWarn);
                            }
                            else
                            {
                                result += GetBasicListItem("The machine was offline most of the day.", colWarn);
                            }
                        }
                        else
                        {
                            result += GetBasicListItem("No analysis available for this dataset.", colWarn);
                        }
                    }
                }

                result += "</ul></div>";
                return result;
            }

            private string GetBasicListItem(string message, string hexCodeColour)
            {
                return $"<li style='padding: 10px; '><p style='margin: 0;padding: 0;color: {hexCodeColour};'>{message}</p></li>";
            }

            private string GetErrorMessageListItem(MachineOperatingBlock errorBlock)
            {
                string message;

                if (IsOutOfHours(errorBlock.StateEntered))
                {
                    message = $"The machine encountered an error when outside of working hours, from {errorBlock.StateEntered:HH:mm} to {errorBlock.StateLeft:HH:mm}.";
                }
                else
                {
                    message = $"The machine encountered an error within working hours, from {errorBlock.StateEntered:HH:mm} to {errorBlock.StateLeft:HH:mm}.";
                }

                if (errorBlock.GetListOfErrors().Count > 0)
                {
                    message += " Here are the error message(s):";
                }
                string result = $"<li style='padding: 10px; '><p style='margin: 0;padding: 0;color: #b71c1c;'>{message}</p>";
                if (errorBlock.GetListOfErrors().Count > 0)
                {
                    List<string> errors = errorBlock.GetListOfErrors();

                    result += "<div style='margin-left: 5%; width: fit-content; padding: 10px;'><ul style='list-style: square; font-size: 10pt; font-family: monospace; color: #333; margin-top: 5px;'>";
                    foreach (string error in errors)
                    {
                        result += $"<li><p style='margin: 0; padding: 0;'>{error}</p></li>";
                    }
                    result += "</ul></div>";
                }

                result += "</li>";
                return result;
            }

            public bool IsOutOfHours(DateTime date)
            {
                if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek < DayOfWeek.Friday) // Mon-thu
                {
                    return date.Hour <= 6 || date.Hour >= 22;
                }
                else if (date.DayOfWeek == DayOfWeek.Friday)
                {
                    return date.Hour <= 6 || date.Hour >= 17;
                }
                else
                {
                    return true;
                }
            }

            public string GetEmailFooter()
            {
                return $"<footer><div style='width: 100%; text-align: center; margin-top: 30px;'><p style='color: lightgray;'>Lighthouse Software &copy; Wixroyd {DateTime.Now.Year}</p></div></footer>";
            }

            private static string FormatDateForEmailSubject(DateTime date)
            {
                int dayOfMonth = date.Day;
                string ordinal;

                ordinal = dayOfMonth switch
                {
                    1 or 21 or 31 => "st",
                    2 or 22 => "nd",
                    3 or 23 => "rd",
                    _ => "th",
                };

                return $"{date:dddd} {dayOfMonth}{ordinal} {date:MMMM}";
            }
        }

        private static bool DateWithinRange(DateTime inputDate, DateTime startingDate)
        {
            double diff = (inputDate - startingDate).TotalHours;
            return diff >= 0 && diff < 24;
        }
    }
}
