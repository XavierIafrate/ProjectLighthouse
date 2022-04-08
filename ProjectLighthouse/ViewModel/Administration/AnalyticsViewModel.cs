using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Globalization;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars
        private AnalyticsHelper analytics;

        public AnalyticsHelper Analytics
        {
            get { return analytics; }
            set
            {
                analytics = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public AnalyticsViewModel()
        {
            Analytics = new();
        }

        #region Old
        public void GenerateReport(string type = null)
        {
            //List<MachineOperatingBlock> segmentedData = MachinePerformanceHelper.SplitBlocksIntoDays(MachineStatistics, 8, 0);

            //DateTime reportStartDate;
            //DateTime reportEndDate;

            //switch (type)
            //{
            //    case "daily":
            //        reportStartDate = DateTime.Now.AddDays(-1).Date.AddHours(8);
            //        reportEndDate = reportStartDate.AddDays(1);
            //        break;
            //    case "weekly":
            //        reportStartDate = DateTime.Now.AddDays(-7).AddDays((((int)DateTime.Now.DayOfWeek) * -1) + 1).Date.AddHours(8);
            //        reportEndDate = reportStartDate.AddDays(7);
            //        break;
            //    default:
            //        reportStartDate = DateTime.Now.AddDays(-7).AddDays((((int)DateTime.Now.DayOfWeek) * -1) + 1).Date.AddHours(8);
            //        reportEndDate = reportStartDate.AddDays(7);
            //        break;
            //}

            //int days = (reportEndDate - reportStartDate).Days;

            //segmentedData = segmentedData
            //    .Where(d => d.StateLeft >= reportStartDate && d.StateEntered < reportEndDate)
            //    .ToList();

            //ReportPdf reportService = new();
            //PerformanceReportData reportData = new()
            //{
            //    FromDate = reportStartDate,
            //    ToDate = reportEndDate,
            //    Days = new(days),
            //};

            //for (int i = 0; i < days; i++)
            //{
            //    DateTime _date = reportStartDate.AddDays(i);
            //    DailyPerformance _dailyPerformance = new()
            //    {
            //        Date = _date,
            //        Deliveries = Deliveries.Where(d => DateWithinRange(d.Header.DeliveryDate, _date)).ToArray(),
            //        LathePerformance = new DailyPerformanceForLathe[Lathes.Count]
            //    };

            //    DailyPerformanceForLathe _dailyPerformanceForLathe;

            //    for (int j = 0; j < Lathes.Count; j++)
            //    {
            //        _dailyPerformanceForLathe = new()
            //        {
            //            Lathe = Lathes[j],
            //            Lots = Lots
            //                .Where(l => DateWithinRange(l.DateProduced, _date)
            //                    && l.FromMachine == Lathes[j].Id).ToArray(),
            //            OperatingBlocks = segmentedData
            //                .Where(d => d.MachineID == Lathes[j].Id
            //                    && DateWithinRange(d.StateEntered, _date)).ToArray(),
            //            Orders = LatheOrders
            //                .Where(o => o.AllocatedMachine == Lathes[j].Id 
            //                    && o.StartDate.Date == _date.Date).ToArray()

            //        };

            //        _dailyPerformance.LathePerformance[j] = _dailyPerformanceForLathe;
            //    }

            //    reportData.Days.Add(_dailyPerformance);

            //}

            //string path = GetTempPdfPath();

            //reportService.Export(path, reportData);
            //reportService.OpenPdf(path);
        }

        private static bool DateWithinRange(DateTime inputDate, DateTime startingDate)
        {
            double diff = (inputDate - startingDate).TotalHours;
            return diff >= 0 && diff < 24;
        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }

        public void GetData()
        {
            //LatheOrders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            //Lots = DatabaseHelper.Read<Lot>().ToList();
            //TurnedProducts = DatabaseHelper.Read<TurnedProduct>().ToList();
            //Lathes = DatabaseHelper.Read<Lathe>().ToList();

            //MachineStatistics = DatabaseHelper.Read<MachineOperatingBlock>().OrderBy(s => s.StateEntered).ToList();

            //// MachineStatistics will be incomplete, need to calculate the incomplete stuff
            //List<MachineStatistics> lastKnownStates = new();
            //lastKnownStates = MachineStatsHelper.GetStats() ?? new();

            //List<MachineOperatingBlock> incompleteBlocks = new();
            //DateTime now = DateTime.Now;
            //foreach (MachineStatistics state in lastKnownStates)
            //{
            //    MachineOperatingBlock lastCompleteBlock = MachineStatistics.Last(s => s.MachineID == state.MachineID);
            //    incompleteBlocks.Add(new()
            //    {
            //        MachineID = state.MachineID,
            //        MachineName = state.MachineName,
            //        State = state.Status,
            //        StateEntered = lastCompleteBlock.StateLeft,
            //        StateLeft = now,
            //        SecondsElapsed = (now - lastCompleteBlock.StateLeft).TotalSeconds,
            //    });
            //}

            //MachineStatistics.AddRange(incompleteBlocks);

            //List<DeliveryNote> deliveryHeaders = DatabaseHelper.Read<DeliveryNote>().ToList();
            //List<DeliveryItem> deliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();

            //Deliveries = new();

            //foreach (DeliveryNote deliveryNote in deliveryHeaders)
            //{
            //    Deliveries.Add(new Delivery()
            //    {
            //        Header = deliveryNote,
            //        Items = deliveryItems.Where(i => i.AllocatedDeliveryNote == deliveryNote.Name).ToArray()
            //    });
            //}
        }

        //public DashboardStats ComputeDashboard()
        //{
        //    DashboardStats newStats = new();

        //    //newStats.TotalOrders = LatheOrders.Count;
        //    //newStats.TotalLots = Lots.Count;
        //    //newStats.DeliveredLots = Lots.Where(n => n.IsDelivered).ToList().Count;

        //    //CartesianMapper<ChartModel> dayConfig = Mappers.Xy<ChartModel>()
        //    //               .X(dayModel => dayModel.DateTime.Ticks)
        //    //               .Y(dayModel => dayModel.Value);


        //    //DateTime now = DateTime.Now;

        //    //SeriesCollection = new SeriesCollection(dayConfig)
        //    //{
        //    //    new LineSeries()
        //    //    {
        //    //        Title="Sum of Parts Made",
        //    //        Values = new ChartValues<ChartModel>(),
        //    //        LineSmoothness=0,
        //    //        PointGeometrySize=0
        //    //    }
        //    //};

        //    //Lots = Lots.OrderBy(n => n.Date).ToList();
        //    //DateTime _date = Lots.First().Date;
        //    //ChartModel _data_point;

        //    //// Last 5 weeks
        //    //WeekLabels = new string[6];
        //    //for (int i = 0; i < 6; i++) // labels
        //    //{
        //    //    WeekLabels[i] = GetIso8601WeekOfYear(DateTime.Now.AddDays((6 - i) * 7 * -1)).ToString();
        //    //}

        //    //List<Lot> recentLots = Lots.Where(n => n.Date.AddDays(7 * 7) > DateTime.Now).ToList();
        //    //List<ValueByWeekNumber> assortedValues = new();
        //    //foreach (Lot l in recentLots)
        //    //{
        //    //    if (l.IsReject || !l.IsDelivered)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    List<LatheManufactureOrder> matchedOrders = LatheOrders.Where(n => n.Name == l.Order).ToList();
        //    //    if (matchedOrders.Count == 0)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    List<TurnedProduct> matchedProducts = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();
        //    //    if (matchedProducts.Count == 0)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    LatheManufactureOrder order = matchedOrders.First();
        //    //    TurnedProduct product = matchedProducts.First();

        //    //    if (product.SellPrice <= 0)
        //    //    {
        //    //        product.SellPrice = 200; //pennies
        //    //    }

        //    //    int weekNum = GetIso8601WeekOfYear(l.Date);
        //    //    if (WeekLabels.Contains(weekNum.ToString()))
        //    //    {
        //    //        double value = product.SellPrice * l.Quantity / 100;
        //    //        ValueByWeekNumber tmp = new(value, order.AllocatedMachine, weekNum, l.Date.Year);
        //    //        assortedValues.Add(tmp);
        //    //    }
        //    //}

        //    //OnPropertyChanged(nameof(WeekLabels));
        //    //FiveWeekValueCollection = new();

        //    //foreach (Lathe lathe in Lathes)
        //    //{
        //    //    List<ValueByWeekNumber> latheDataSet = assortedValues.Where(n => n.MachineID == lathe.Id).OrderBy(x => x.Week).ThenBy(x => x.Year).ToList();
        //    //    if (latheDataSet.Count == 0)
        //    //        continue;

        //    //    ColumnSeries tmpSeries = new()
        //    //    {
        //    //        Title = lathe.FullName,
        //    //        Values = new ChartValues<double> { },
        //    //        ColumnPadding = 16
        //    //    };

        //    //    foreach (string week in WeekLabels)
        //    //    {
        //    //        tmpSeries.Values.Add(latheDataSet.Where(l => l.Week.ToString() == week).Sum(m => m.Value));
        //    //    }
        //    //    FiveWeekValueCollection.Add(tmpSeries);
        //    //}

        //    ////Total Lots, parts, Value
        //    //foreach (Lot l in Lots)
        //    //{
        //    //    if (!l.IsDelivered)
        //    //        continue;
        //    //    newStats.TotalPartsMade += l.Quantity;
        //    //    if (l.Date.Year == DateTime.Now.Year)
        //    //        newStats.TotalPartsMadeThisYear += l.Quantity;

        //    //    List<TurnedProduct> deliveredProduct = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();
        //    //    List<LatheManufactureOrder> assignedOrder = LatheOrders.Where(n => n.Name == l.ProductName).ToList();
        //    //    double price;
        //    //    if (deliveredProduct.Count == 0)
        //    //    {
        //    //        newStats.TotalValue += l.Quantity * 2;
        //    //    }
        //    //    else
        //    //    {
        //    //        price = deliveredProduct.First().SellPrice == 0 ? 200 : deliveredProduct.First().SellPrice;
        //    //        newStats.TotalValue += l.Quantity * price / 100;
        //    //    }

        //    //    DateTime _lotDate = new(l.Date.Year, l.Date.Month, l.Date.Day);
        //    //    if (_lotDate > _date)
        //    //    {
        //    //        _data_point = new(_date, newStats.TotalPartsMade);
        //    //        SeriesCollection[0].Values.Add(_data_point);
        //    //        _date = _lotDate;
        //    //    }
        //    //}

        //    //_data_point = new(_date, newStats.TotalPartsMade);

        //    //CelebrationVisibility = (newStats.TotalPartsMade is >= 1000000 and <= 1015000)
        //    //    ? Visibility.Visible
        //    //    : Visibility.Collapsed;

        //    //SeriesCollection[0].Values.Add(_data_point);

        //    //GetDataForMachineRuntime();

        //    return newStats;
        //}

        private void GetDataForMachineRuntime()
        {
            //string[] StateLabels = new[] { "Running", "Setting", "Breakdown", "Idle", "Offline" };
            //List<string> lathe_labels = new();
            //MachineRuntimeCollection = new();

            //List<StackedRowSeries> temporal = new();

            //// Create Series
            //for (int i = 0; i < StateLabels.Length; i++)
            //{
            //    StackedRowSeries stateSummary = new()
            //    {
            //        Title = StateLabels[i],
            //        Values = new ChartValues<double>() { },
            //        StackMode = StackMode.Percentage,
            //        RowPadding = 5,
            //    };
            //    temporal.Add(stateSummary);
            //}

            //// Get Data

            //double OverallRuntime = 0;
            //double OverallTime = 0;

            //double startTimeSpan = 24 * 7; // hours ago

            //for (int lathe = Lathes.Count - 1; lathe >= 0; lathe--)
            //{
            //    List<MachineOperatingBlock> data = MachineStatistics.Where(n => n.MachineID == Lathes[lathe].Id && n.StateLeft.AddHours(startTimeSpan) > DateTime.Now).OrderBy(x => x.StateEntered).ToList();
            //    if (data.Count == 0)
            //        continue;

            //    lathe_labels.Add(Lathes[lathe].FullName);

            //    double sRunning = 0;
            //    double sSetting = 0;
            //    double sBreakdown = 0;
            //    double sIdle = 0;
            //    double sOffline = 0;

            //    for (int i = 1; i < data.Count; i++)
            //    {
            //        DateTime last_time;

            //        if (i == 0)
            //        {
            //            last_time = DateTime.Now.AddHours(-startTimeSpan) > data[i].StateEntered
            //                ? DateTime.Now.AddHours(-startTimeSpan)
            //                : data[i].StateEntered;
            //        }
            //        else
            //        {
            //            last_time = data[i].StateEntered;
            //        }

            //        switch (data[i].State)
            //        {
            //            case "Running":
            //                sRunning += (data[i].StateLeft - last_time).TotalSeconds;
            //                break;
            //            case "Setting":
            //                sSetting += (data[i].StateLeft - last_time).TotalSeconds;
            //                break;
            //            case "Breakdown":
            //                sBreakdown += (data[i].StateLeft - last_time).TotalSeconds;
            //                break;
            //            case "Idle":
            //                sIdle += (data[i].StateLeft - last_time).TotalSeconds;
            //                break;
            //            case "Offline":
            //                sOffline += (data[i].StateLeft - last_time).TotalSeconds;
            //                break;
            //        }
            //    }

            //    OverallTime += sRunning + sSetting + sBreakdown + sIdle + sOffline;

            //    BrushConverter converter = new();
            //    foreach (StackedRowSeries state in temporal)
            //    {
            //        switch (state.Title)
            //        {
            //            case "Running":
            //                state.Values.Add(sRunning);
            //                OverallRuntime += sRunning;
            //                state.Fill = (Brush)converter.ConvertFromString("#009688");
            //                break;
            //            case "Setting":
            //                state.Values.Add(sSetting);
            //                state.Fill = (Brush)converter.ConvertFromString("#0277BD");
            //                break;
            //            case "Breakdown":
            //                state.Values.Add(sBreakdown);
            //                state.Fill = (Brush)converter.ConvertFromString("#B00020");
            //                break;
            //            case "Idle":
            //                state.Values.Add(sIdle);
            //                state.Fill = (Brush)converter.ConvertFromString("#ffff71");
            //                break;
            //            case "Offline":
            //                state.Values.Add(sOffline);
            //                state.Fill = (Brush)converter.ConvertFromString("#000000");
            //                break;
            //        }
            //    }
            //}

            //double overall = Math.Round((OverallRuntime / OverallTime) * 100, 2);
            //OverallOperating = double.IsNaN(overall)
            //    ? 0
            //    : overall;
            //OnPropertyChanged(nameof(OverallOperating));

            ////SecondsToDaysFormatter = value => string.Format("{0:d}", TimeSpan.FromSeconds(value));
            //LatheLabels = lathe_labels.ToArray();
            //OnPropertyChanged(nameof(LatheLabels));
            //MachineRuntimeCollection.AddRange(temporal);

            //OnPropertyChanged(nameof(MachineRuntimeCollection));

        }

        public static int GetIso8601WeekOfYear(DateTime time) // shamelessly stolen
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Tuesday);
        }
        #endregion
    }
}
