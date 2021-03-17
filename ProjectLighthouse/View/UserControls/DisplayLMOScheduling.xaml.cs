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
    /// Interaction logic for DisplayLMOScheduling.xaml
    /// </summary>
    public partial class DisplayLMOScheduling : UserControl
    {



        public LatheManufactureOrder order
        {
            get { return (LatheManufactureOrder)GetValue(orderProperty); }
            set { SetValue(orderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty orderProperty =
            DependencyProperty.Register("order", typeof(LatheManufactureOrder), typeof(DisplayLMOScheduling), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLMOScheduling control = d as DisplayLMOScheduling;

            if (control != null)
            {
                control.DataContext = control.order;
            }
        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }
    }
}
