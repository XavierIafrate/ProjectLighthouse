using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

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
            if (d is not DisplayMachineLiveChart control)
                return;

            control.DataContext = control.dataSet;


        }

        public DisplayMachineLiveChart()
        {
            InitializeComponent();
        }
    }
}
