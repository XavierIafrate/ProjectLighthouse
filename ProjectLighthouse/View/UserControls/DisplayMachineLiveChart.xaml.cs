using LiveCharts;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayMachineLiveChart.xaml
    /// </summary>
    public partial class DisplayMachineLiveChart : UserControl
    {


        public MachineLiveChartModel dataSet
        {
            get { return (MachineLiveChartModel)GetValue(dataSetProperty); }
            set { SetValue(dataSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for dataSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty dataSetProperty =
            DependencyProperty.Register("dataSet", typeof(MachineLiveChartModel), typeof(DisplayMachineLiveChart), new PropertyMetadata(null, SetValues));



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayMachineLiveChart control = d as DisplayMachineLiveChart;
            if (control == null)
                return;

            control.DataContext = control.dataSet;


        }

        public DisplayMachineLiveChart()
        {
            InitializeComponent();
        }
    }
}
