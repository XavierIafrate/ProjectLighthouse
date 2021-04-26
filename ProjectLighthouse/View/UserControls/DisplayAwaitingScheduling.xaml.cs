using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

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
            }
        }

        public DisplayAwaitingScheduling()
        {
            InitializeComponent();
        }
    }
}
