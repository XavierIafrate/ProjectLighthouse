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
    /// Interaction logic for DisplayStatsCard.xaml
    /// </summary>
    public partial class DisplayStatsCard : UserControl
    {



        public MachineStatistics stats
        {
            get { return (MachineStatistics)GetValue(statsProperty); }
            set { SetValue(statsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for stats.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty statsProperty =
            DependencyProperty.Register("stats", typeof(MachineStatistics), typeof(DisplayStatsCard), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayStatsCard control = d as DisplayStatsCard;

            if(control.stats != null)
            {
                control.DataContext = control.stats;
            }
        }

        public DisplayStatsCard()
        {
            InitializeComponent();
        }
    }
}
