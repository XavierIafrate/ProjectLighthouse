using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class MachineStatsViewModel : BaseViewModel
    {
        #region Vars
        public List<MachineStatistics> StatsList { get; set; }
        public List<Lathe> Lathes { get; set; }
        public Visibility NoConnectionVis { get; set; }
        public Visibility StatsVis { get; set; }
        private List<MachineLiveChartModel> cardInfo;

        public List<MachineLiveChartModel> CardInfo
        {
            get
            {
                return cardInfo;
            }
            set
            {
                cardInfo = value;
                OnPropertyChanged("CardInfo");
            }
        }
        #endregion

        public MachineStatsViewModel()
        {
            Debug.WriteLine("Init: MachineStatsViewModel");
            NoConnectionVis = Visibility.Hidden;
            StatsVis = Visibility.Visible;
            CardInfo = new();
            LoadDataAsync();
        }

        public async void LoadDataAsync()
        {
            await Task.Run(function: () => GetData());
            GetStats();
            //CardInfo = await GetStats();
        }

        public async Task GetData()
        {
            StatsList = DatabaseHelper.Read<MachineStatistics>().Where(n => n.DataTime > DateTime.Now.AddDays(-2)).ToList();
            Lathes = DatabaseHelper.Read<Lathe>();
        }

        public void GetStats()
        {

            List<MachineLiveChartModel> results = new();
            var dayConfig = Mappers.Xy<ChartModel>()
                           .X(dayModel => dayModel.DateTime.Ticks)
                           .Y(dayModel => dayModel.Value);

            ChartModel _dataPoint;
            foreach (Lathe lathe in Lathes)
            {
                MachineLiveChartModel machineStatsModel = new();
                List<MachineStatistics> relevantStats = StatsList.Where(n => n.MachineID == lathe.Id).OrderBy(m => m.DataTime).ToList();
                if (relevantStats.Count == 0)
                    continue;


                machineStatsModel.series = new(dayConfig)
                {
                    new LineSeries()
                    {
                        Title = lathe.FullName,
                        Values = new ChartValues<ChartModel>(),
                        LineSmoothness = 0,
                        PointGeometrySize = 0
                    }
                };

                DateTime start = DateTime.Now;


                var temporalValues = new ChartModel[relevantStats.Count];
                int i = 0;

                foreach (MachineStatistics stat in relevantStats)
                {
                    _dataPoint = new(stat.DataTime, stat.PartCountAll);
                    //machineStatsModel.series[0].Values.Add(_dataPoint); // saved 2ms!
                    temporalValues[i] = _dataPoint;
                    i++;
                }

                machineStatsModel.series[0].Values.AddRange(temporalValues);

                Debug.WriteLine($"Finished in {(DateTime.Now - start).TotalMilliseconds}ms");

                machineStatsModel.partCounterAll = relevantStats.Last().PartCountAll;
                machineStatsModel.tick2 = temporalValues[1].DateTime;
                machineStatsModel.partCounterTarget = relevantStats.Last().PartCountTarget;
                machineStatsModel.dataReadAt = relevantStats.Last().DataTime;
                machineStatsModel.title = lathe.FullName;

                results.Add(machineStatsModel);
            }
            CardInfo = results;
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
