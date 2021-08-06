using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars
        private List<Lot> Lots { get; set; }
        private List<LatheManufactureOrder> LatheOrders { get; set; }
        //private List<LatheManufactureOrderItem> LatheOrderItems { get; set; }
        //private List<DeliveryNote> DeliveryNotes { get; set; }
        //private List<DeliveryItem> DeliveryItems { get; set; }
        private List<TurnedProduct> TurnedProducts { get; set; }
        private List<Lathe> Lathes { get; set; }
        private List<MachineStatistics> MachineStatistics { get; set; }

        //public DateTime InitialDateTime { get; set; }
        private SeriesCollection seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get { return seriesCollection; }
            set
            {
                seriesCollection = value;
                OnPropertyChanged("SeriesCollection");
            }
        }

        public string[] WeekLabels { get; set; }
        private SeriesCollection fiveWeekValueCollection;
        public SeriesCollection FiveWeekValueCollection
        {
            get { return fiveWeekValueCollection; }
            set
            {
                fiveWeekValueCollection = value;
                OnPropertyChanged("FiveWeekValueCollection");
            }
        }

        public string[] LatheLabels { get; set; }
        
        private SeriesCollection machineRuntimeCollection;
        public SeriesCollection MachineRuntimeCollection
        {
            get { return machineRuntimeCollection; }
            set
            {
                machineRuntimeCollection = value;
                OnPropertyChanged("MachineRuntimeCollection");
            }
        }
        public double OverallOperating { get; set; }
        public Func<double, string> PercentageStringFormat { get; set; }


        private DashboardStats stats;
        public DashboardStats Stats
        {
            get { return stats; }
            set
            {
                stats = value;
                Debug.WriteLine($"Stats total parts: {value.TotalPartsMade}");
                OnPropertyChanged("Stats");
            }
        }

        #endregion
        public AnalyticsViewModel()
        {
            Debug.WriteLine("Init: AnalyticsViewModel");
            LoadData();
        }

        private async void LoadData()
        {
            DateTime start = DateTime.Now;
            await Task.Run(function: () => GetData());

            

            Stats = new();
            Stats = ComputeDashboard();

            //OnPropertyChanged("SeriesCollection");
            Debug.WriteLine($"Data computed at time {(DateTime.Now - start).TotalMilliseconds:N0} ms");
        }

        public async Task GetData()
        {
            LatheOrders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            //LatheOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            //DeliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();
            //DeliveryNotes = DatabaseHelper.Read<DeliveryNote>().ToList();
            Lots = DatabaseHelper.Read<Lot>().ToList();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>().ToList();
            Lathes = DatabaseHelper.Read<Lathe>().ToList();
            MachineStatistics = DatabaseHelper.Read<MachineStatistics>().ToList();
        }

        public DashboardStats ComputeDashboard()
        {
            DashboardStats newStats = new();

            newStats.TotalOrders = LatheOrders.Count;
            newStats.TotalLots = Lots.Count;
            newStats.DeliveredLots = Lots.Where(n => n.IsDelivered).ToList().Count;

            CartesianMapper<ChartModel> dayConfig = Mappers.Xy<ChartModel>()
                           .X(dayModel => dayModel.DateTime.Ticks)
                           .Y(dayModel => dayModel.Value);


            DateTime now = DateTime.Now;

            SeriesCollection = new SeriesCollection(dayConfig)
            {
                new LineSeries()
                {
                    Title="Sum of Parts Made",
                    Values = new ChartValues<ChartModel>(),
                    LineSmoothness=0,
                    PointGeometrySize=0
                }
            };

            

            Lots = Lots.OrderBy(n => n.Date).ToList();
            DateTime _date = Lots.First().Date;
            ChartModel _data_point;

            // Last 5 weeks
            WeekLabels = new string[6];
            for (int i = 0; i < 6; i++) // labels
            {
                WeekLabels[i] = GetIso8601WeekOfYear(DateTime.Now.AddDays((6 - i) * 7 * -1)).ToString();
                //Debug.WriteLine($"Week Num: {WeekLabels[i]}");
            }

            List<Lot> recentLots = Lots.Where(n => n.Date.AddDays(7 * 7) > DateTime.Now).ToList();
            List<ValueByWeekNumber> assortedValues = new();
            foreach (Lot l in recentLots)
            {
                if (l.IsReject || !l.IsDelivered)
                    continue;
                List<LatheManufactureOrder> matchedOrders = LatheOrders.Where(n => n.Name == l.Order).ToList();
                if (matchedOrders.Count == 0)
                    continue;

                List<TurnedProduct> matchedProducts = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();
                if (matchedProducts.Count == 0)
                    continue;

                LatheManufactureOrder order = matchedOrders.First();
                TurnedProduct product = matchedProducts.First();

                if (product.SellPrice <= 0)
                    product.SellPrice = 200; //pennies


                int weekNum = GetIso8601WeekOfYear(l.Date);
                if (WeekLabels.Contains(weekNum.ToString()))
                {
                    double value = product.SellPrice * l.Quantity / 100;
                    ValueByWeekNumber tmp = new(value, order.AllocatedMachine, weekNum);
                    assortedValues.Add(tmp);
                }
            }

            OnPropertyChanged("WeekLabels");
            FiveWeekValueCollection = new();

            foreach (Lathe lathe in Lathes)
            {
                List<ValueByWeekNumber> latheDataSet = assortedValues.Where(n => n.MachineID == lathe.Id).OrderBy(x => x.Week).ToList();
                if (latheDataSet.Count == 0)
                    continue;

                ColumnSeries tmpSeries = new()
                {
                    Title = lathe.FullName,
                    Values = new ChartValues<double> { },
                    ColumnPadding = 16
                };

                foreach (string week in WeekLabels)
                {
                    tmpSeries.Values.Add(latheDataSet.Where(l => l.Week.ToString() == week).Sum(m => m.Value));
                }
                FiveWeekValueCollection.Add(tmpSeries);
            }

            //Total Lots, parts, Value
            foreach (Lot l in Lots)
            {
                if (!l.IsDelivered)
                    continue;
                newStats.TotalPartsMade += l.Quantity;
                if (l.Date.Year == DateTime.Now.Year)
                    newStats.TotalPartsMadeThisYear += l.Quantity;

                List<TurnedProduct> deliveredProduct = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();
                List<LatheManufactureOrder> assignedOrder = LatheOrders.Where(n => n.Name == l.ProductName).ToList();
                double price;
                if (deliveredProduct.Count == 0)
                {
                    newStats.TotalValue += l.Quantity * 2;
                }
                else
                {
                    price = deliveredProduct.First().SellPrice == 0 ? 200 : deliveredProduct.First().SellPrice;
                    newStats.TotalValue += l.Quantity * price / 100;
                }

                DateTime _lotDate = new(l.Date.Year, l.Date.Month, l.Date.Day);
                if (_lotDate > _date)
                {
                    _data_point = new(_date, newStats.TotalPartsMade);
                    SeriesCollection[0].Values.Add(_data_point);
                    _date = _lotDate;
                }
            }

            _data_point = new(_date, newStats.TotalPartsMade);
            SeriesCollection[0].Values.Add(_data_point);

            GetDataForMachineRuntime();

            return newStats;
        }

        private void GetDataForMachineRuntime()
        {
            string[] StateLabels = new[] { "Running", "Setting", "Breakdown", "Idle", "Offline" };
            List<string> lathe_labels = new();
            MachineRuntimeCollection = new();

            List<StackedRowSeries> temporal = new();

            // Create Series
            for (int i = 0; i < StateLabels.Length; i++)
            {
                StackedRowSeries stateSummary = new()
                {
                    Title = StateLabels[i],
                    Values = new ChartValues<double>() { },
                    StackMode = StackMode.Percentage,
                    RowPadding = 5,
                };
                temporal.Add(stateSummary);
            }

            // Get Data

            double OverallRuntime = 0;
            double OverallTime = 0;

            for (int lathe = Lathes.Count - 1; lathe >= 0; lathe--)
            {
                List<MachineStatistics> data = MachineStatistics.Where(n => n.MachineID == Lathes[lathe].Id && n.DataTime.AddHours(24 * 7) > DateTime.Now).OrderBy(x => x.DataTime).ToList();
                if (data.Count == 0)
                    continue;

                lathe_labels.Add(Lathes[lathe].FullName);

                DateTime last_time = data[0].DataTime;

                double sRunning = 0;
                double sSetting = 0;
                double sBreakdown = 0;
                double sIdle = 0;
                double sOffline = 0;

                for (int i = 1; i < data.Count; i++)
                {
                    switch (data[i].Status)
                    {
                        case "Running":
                            sRunning += (data[i].DataTime - last_time).TotalSeconds;
                            break;
                        case "Setting":
                            sSetting += (data[i].DataTime - last_time).TotalSeconds;
                            break;
                        case "Breakdown":
                            sBreakdown += (data[i].DataTime - last_time).TotalSeconds;
                            break;
                        case "Idle":
                            sIdle += (data[i].DataTime - last_time).TotalSeconds;
                            break;
                        case "Offline":
                            sOffline += (data[i].DataTime - last_time).TotalSeconds;
                            break;

                    }

                    last_time = data[i].DataTime;
                }

                OverallTime += (sRunning + sSetting + sBreakdown + sIdle + sOffline);

                BrushConverter converter = new BrushConverter();
                foreach (StackedRowSeries state in temporal)
                {

                    switch (state.Title)
                    {
                        case "Running":
                            state.Values.Add(sRunning);
                            OverallRuntime += sRunning;
                            state.Fill = (System.Windows.Media.Brush)converter.ConvertFromString("#009688");
                            break;
                        case "Setting":
                            state.Values.Add(sSetting);
                            state.Fill = (System.Windows.Media.Brush)converter.ConvertFromString("#0277BD");
                            break;
                        case "Breakdown":
                            state.Values.Add(sBreakdown);
                            state.Fill = (System.Windows.Media.Brush)converter.ConvertFromString("#B00020");
                            break;
                        case "Idle":
                            state.Values.Add(sIdle);
                            state.Fill = (System.Windows.Media.Brush)converter.ConvertFromString("#ffff71");
                            break;
                        case "Offline":
                            state.Values.Add(sOffline);
                            state.Fill = (System.Windows.Media.Brush)converter.ConvertFromString("#000000");
                            break;
                    }
                }
            }

            OverallOperating = Math.Round((OverallRuntime / OverallTime) * 100, 2);
            OnPropertyChanged("OverallOperating");

            //SecondsToDaysFormatter = value => string.Format("{0:d}",TimeSpan.FromSeconds(value));
            LatheLabels = lathe_labels.ToArray();
            OnPropertyChanged("LatheLabels");
            MachineRuntimeCollection.AddRange(temporal);

        }

        public static int GetIso8601WeekOfYear(DateTime time) // shamelessly stolen
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                time = time.AddDays(3);

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Tuesday);
        }

        #region Classes
        public class DashboardStats
        {
            public int TotalPartsMade { get; set; }
            public int TotalPartsMadeThisYear { get; set; }
            public int TotalOrders { get; set; }
            public int TotalLots { get; set; }
            public int DeliveredLots { get; set; }
            public double TotalValue { get; set; }
        }

        public class ChartModel
        {
            public DateTime DateTime { get; set; }
            public int Value { get; set; }

            public ChartModel(DateTime dateTime, int value)
            {
                DateTime = dateTime;
                Value = value;
            }
        }

        public class ValueByWeekNumber
        {
            public double Value { get; set; }
            public string MachineID { get; set; }
            public int Week { get; set; }

            public ValueByWeekNumber(double value, string machine, int week)
            {
                Value = value;
                MachineID = machine;
                Week = week;
            }
        }
        #endregion
    }
}
