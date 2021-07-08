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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars
        private List<Lot> Lots { get; set; }
        private List<LatheManufactureOrder> LatheOrders { get; set; }
        private List<LatheManufactureOrderItem> LatheOrderItems { get; set; }
        private List<DeliveryNote> DeliveryNotes { get; set; }
        private List<DeliveryItem> DeliveryItems { get; set; }
        private List<TurnedProduct> TurnedProducts { get; set; }
        private List<Lathe> Lathes { get; set; }

        //public DateTime InitialDateTime { get; set; }
        public Func<double, string> TimeFormatter { get; set; }
        public Func<double, string> QuantityFormatter { get; set; }
        public Func<double, string> ThousandPoundFormatter { get; set; }
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


        private DashboardStats stats;
        public DashboardStats Stats
        {
            get { return stats; }
            set 
            { 
                stats = value;
                Debug.WriteLine($"Stats total parts: {value.totalPartsMade}");
                OnPropertyChanged("Stats");
            }
        }

#endregion
        public AnalyticsViewModel()
        {
            LoadData();
            Debug.WriteLine("end of constructor");
        }

        private async void LoadData()
        {
            DateTime start = DateTime.Now;
            await Task.Run(function: () => GetData());

            Stats = new();
            Stats = ComputeDashboard();

            OnPropertyChanged("SeriesCollection");
            Debug.WriteLine($"Data computed at time {(DateTime.Now - start).TotalMilliseconds:N0} ms");
        }

        public async Task GetData()
        {
            LatheOrders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            LatheOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            DeliveryItems = DatabaseHelper.Read<DeliveryItem>().ToList();
            DeliveryNotes = DatabaseHelper.Read<DeliveryNote>().ToList();
            Lots = DatabaseHelper.Read<Lot>().ToList();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>().ToList();
            Lathes = DatabaseHelper.Read<Lathe>().ToList();
        }

        public DashboardStats ComputeDashboard()
        {
            DashboardStats newStats = new();

            newStats.totalOrders = LatheOrders.Count;
            newStats.totalLots = Lots.Count;
            newStats.deliveredLots = Lots.Where(n => n.IsDelivered).ToList().Count;

            var dayConfig = Mappers.Xy<ChartModel>()
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

            TimeFormatter = value => new DateTime((long)value).ToString("dd MMMM yyyy");
            QuantityFormatter = value => string.Format($"{value:#,##0}");
            ThousandPoundFormatter = value => string.Format($"£{value:#,##0}");

            Lots = Lots.OrderBy(n => n.Date).ToList();
            DateTime _date = Lots.First().Date;
            ChartModel _data_point;

            // Last 5 weeks
            WeekLabels = new string[6];
            for (int i = 0; i < 6; i++) // labels
            {
                WeekLabels[i] = GetIso8601WeekOfYear(DateTime.Now.AddDays((6-i) * 7 * -1)).ToString();
                Debug.WriteLine($"Week Num: {WeekLabels[i]}");
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

            FiveWeekValueCollection = new();

            foreach (Lathe lathe in Lathes)
            {
                List<ValueByWeekNumber> latheDataSet = assortedValues.Where(n => n.MachineID == lathe.Id).OrderBy(x=>x.Week).ToList();
                if (latheDataSet.Count == 0)
                    continue;

                ColumnSeries tmpSeries = new()
                {
                    Title = lathe.FullName,
                    Values = new ChartValues<double> { },
                    ColumnPadding=16
                };

                foreach(string week in WeekLabels)
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
                newStats.totalPartsMade += l.Quantity;
                if (l.Date.Year == DateTime.Now.Year)
                    newStats.totalPartsMadeThisYear += l.Quantity;

                List<TurnedProduct> deliveredProduct = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();
                List<LatheManufactureOrder> assignedOrder = LatheOrders.Where(n => n.Name== l.ProductName).ToList();
                double price;
                if (deliveredProduct.Count == 0)
                {
                    newStats.totalValue += l.Quantity * 2;
                }
                else
                {
                    price = deliveredProduct.First().SellPrice == 0 ? 200 : deliveredProduct.First().SellPrice;
                    newStats.totalValue += l.Quantity * price / 100;
                }

                DateTime _lotDate = new DateTime(l.Date.Year, l.Date.Month, l.Date.Day);
                if (_lotDate > _date)
                {
                    _data_point = new(_date, newStats.totalPartsMade);
                    SeriesCollection[0].Values.Add(_data_point);
                    _date = _lotDate;
                }
            }

            _data_point = new(_date, newStats.totalPartsMade);
            SeriesCollection[0].Values.Add(_data_point);

            //SeriesCollection.Add(cumulativePartsMade);
            return newStats;
        }

 

        public static int GetIso8601WeekOfYear(DateTime time) // shamelessly stolen
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                time = time.AddDays(3);

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Tuesday);
        }

        public class DashboardStats
        {
            public int totalPartsMade { get; set; }
            public int totalPartsMadeThisYear { get; set; }
            public int totalOrders { get; set; }
            public int totalLots { get; set; }
            public int deliveredLots { get; set; }
            public double totalValue { get; set; }
        }

        public class ChartModel
        {
            public DateTime DateTime { get; set; }
            public int Value { get; set; }

            public ChartModel(DateTime dateTime, int value)
            {
                this.DateTime = dateTime;
                this.Value = value;
            }
        }

        public class ValueByWeekNumber
        {
            public double Value { get; set; }
            public string MachineID { get; set; }
            public int Week { get; set; }

            public ValueByWeekNumber(double value, string machine, int week)
            {
                this.Value = value;
                this.MachineID = machine;
                this.Week = week;
            }
        }

    }
}
