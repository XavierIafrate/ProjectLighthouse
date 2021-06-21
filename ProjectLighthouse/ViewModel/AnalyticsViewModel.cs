using LiveCharts;
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


        public Func<double, string> YFormatter { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        
        private string[] _labels;
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
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

            Debug.WriteLine($"Data loaded at time {(DateTime.Now - start).TotalMilliseconds:N0} ms");

            YFormatter = value => value.ToString("#,##0");
            SeriesCollection = new();


            Stats = new();
            Stats = ComputeDashboard();

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
            newStats.totalPartsMade = 100;

            newStats.totalOrders = LatheOrders.Count;
            newStats.totalLots = Lots.Count;
            newStats.deliveredLots = Lots.Where(n => n.IsDelivered).ToList().Count;

            newStats.time = new();
            newStats.c_sum = new();
            newStats.TimeFormatter = this.DateLabelFormatter;
            newStats.ThousandsFormatter = value => new string($"{value:#,##0}");

            Labels = Array.Empty<string>();

            LineSeries TotalPartsOverTime = new();
            TotalPartsOverTime.Title = "Number of parts made over time";
            ChartValues<double> values = new();


            for(int i = 0; i<=20; i++)
            {
                newStats.totalPartsMade += 10;
                newStats.c_sum.Add(newStats.totalPartsMade);

                values.Add(newStats.totalPartsMade);
                //newStats.time.Add(DateTime.Now.AddDays(-1*i));
                Labels.Append(DateTime.Now.AddDays(-1 * i).ToString("MMMM yy"));
            }

            TotalPartsOverTime.Values = values;

            SeriesCollection.Add(TotalPartsOverTime);


            //foreach (Lot l in Lots)
            //{
            //    if(l.IsDelivered)
            //    {
            //        newStats.totalPartsMade += l.Quantity;
            //        if (l.Date.Year == DateTime.Now.Year)
            //            newStats.totalPartsMadeThisYear += l.Quantity;

            //        newStats.time.Add(l.Date);
            //        Debug.WriteLine($"date: {l.Date}");
            //        newStats.c_sum.Add(newStats.totalPartsMade);
            //    }
                    
            //}
            return newStats;
        }

        private string DateLabelFormatter(double value)
        {
            DateTime dateTime = new DateTime((long)(value * TimeSpan.FromSeconds(1).Ticks));
            return dateTime.ToString("dd/MM/yy");
        }

        public class DashboardStats
        {
            public int totalPartsMade { get; set; }
            public int totalPartsMadeThisYear { get; set; }
            public int totalOrders { get; set; }
            public int totalLots { get; set; }
            public int deliveredLots { get; set; }

            public Func<double, string> TimeFormatter { get; set; }
            public Func<double, string> ThousandsFormatter { get; set; }

            public ChartValues<DateTime> time { get; set; }
            public ChartValues<int> c_sum { get; set; }
            public string[] timeLabels { get; set; }
        }
    }
}
