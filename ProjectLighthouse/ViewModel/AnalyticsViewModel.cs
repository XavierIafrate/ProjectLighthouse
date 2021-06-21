using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class AnalyticsViewModel : BaseViewModel
    {
        private List<Lot> Lots { get; set; }
        private List<LatheManufactureOrder> LatheOrders { get; set; }
        private List<LatheManufactureOrderItem> LatheOrderItems { get; set; }
        private List<DeliveryNote> DeliveryNotes { get; set; }
        private List<DeliveryItem> DeliveryItems { get; set; }
        private List<TurnedProduct> TurnedProducts { get; set; }

        public DateTime InitialDateTime { get; set; }
        public Func<double, string> TimeFormatter { get; set; }
        public Func<double, string> QuantityFormatter { get; set; }
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

            Lots = Lots.OrderBy(n => n.Date).ToList();
            DateTime _date = Lots.First().Date;
            ChartModel _data_point;

            foreach (Lot l in Lots)
            {
                if (l.IsDelivered)
                {
                    newStats.totalPartsMade += l.Quantity;
                    if (l.Date.Year == DateTime.Now.Year)
                        newStats.totalPartsMadeThisYear += l.Quantity;

                    List<TurnedProduct> deliveredProduct = TurnedProducts.Where(n => n.ProductName == l.ProductName).ToList();

                    if(deliveredProduct.Count == 0)
                    {
                        newStats.totalValue += l.Quantity * 2;
                    }
                    else
                    {
                        double price = deliveredProduct.First().SellPrice == 0 ? 200 : deliveredProduct.First().SellPrice;
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
            }

            _data_point = new(_date, newStats.totalPartsMade);
            SeriesCollection[0].Values.Add(_data_point);


            //SeriesCollection.Add(cumulativePartsMade);
            return newStats;
        }

        private string DateLabelFormatter(double value)
        {
            // * TimeSpan.FromSeconds(1).Ticks
            DateTime dateTime = new DateTime((long)(value));
            return dateTime.ToString("dd/MM/yy");
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
    }
}
