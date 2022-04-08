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
using static ProjectLighthouse.ViewModel.Helpers.AnalyticsHelper;

namespace ProjectLighthouse.View.AdministrationViews.AnalyticsComponents
{
    public partial class DisplayDaysPerformance : UserControl
    {


        public OperatingPerformance Performance
        {
            get { return (OperatingPerformance)GetValue(PerformanceProperty); }
            set { SetValue(PerformanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Performance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(OperatingPerformance), typeof(DisplayDaysPerformance), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDaysPerformance control)
            {
                return;
            }

            control.DataContext = control.Performance;
        }

        public DisplayDaysPerformance()
        {
            InitializeComponent();
        }
    }
}
