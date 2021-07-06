using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class MachineStatsViewModel : BaseViewModel
    {
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

        public MachineStatsViewModel()
        {
            NoConnectionVis = Visibility.Hidden;
            StatsVis = Visibility.Visible;
            CardInfo = new();
            _ = LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            CardInfo = getStats();
        }

        public static List<MachineLiveChartModel> getStats()
        {

            List<MachineLiveChartModel> results = new();
            var dayConfig = Mappers.Xy<ChartModel>()
                           .X(dayModel => dayModel.DateTime.Ticks)
                           .Y(dayModel => dayModel.Value);

            List<MachineStatistics> statsList = DatabaseHelper.Read<MachineStatistics>().Where(n=>n.DataTime > DateTime.Now.AddDays(-2)).ToList();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
            
            ChartModel _dataPoint;
            foreach (Lathe lathe in lathes)
            {
                MachineLiveChartModel machineStatsModel = new();
                List<MachineStatistics> relevantStats = statsList.Where(n => n.MachineID == lathe.Id).OrderBy(m=>m.DataTime).ToList();
                if (relevantStats.Count == 0)
                    continue;


                machineStatsModel.series = new(dayConfig)
                {
                    new LineSeries()
                    {
                        Title=lathe.FullName,
                        Values = new ChartValues<ChartModel>(),
                        LineSmoothness=0,
                        PointGeometrySize=0
                    }
                };

                foreach(MachineStatistics stat in relevantStats) 
                {
                    _dataPoint = new(stat.DataTime, stat.PartCountAll);
                    machineStatsModel.series[0].Values.Add(_dataPoint);
                }

                machineStatsModel.partCounterAll = relevantStats.Last().PartCountAll;
                machineStatsModel.partCounterTarget = relevantStats.Last().PartCountTarget;
                machineStatsModel.dataReadAt = relevantStats.Last().DataTime;
                machineStatsModel.title = lathe.FullName;

                results.Add(machineStatsModel);
            }
            return results;
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
