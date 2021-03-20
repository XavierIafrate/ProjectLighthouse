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
    /// Interaction logic for DisplayAwaitingScheduling.xaml
    /// </summary>
    public partial class DisplayAwaitingScheduling : UserControl
    {


        public LatheManufactureOrder order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("order", typeof(LatheManufactureOrder), typeof(DisplayAwaitingScheduling), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAwaitingScheduling control = d as DisplayAwaitingScheduling;

            if (control != null)
            {
                control.DataContext = control.order;
                control.bg.Fill = Brushes.Black;
            }
        }

        public DisplayAwaitingScheduling()
        {
            InitializeComponent();
        }
    }
}
