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
using ProjectLighthouse.Model.Assembly;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayRouting.xaml
    /// </summary>
    public partial class DisplayRouting : UserControl
    {
        public Routing routing
        {
            get { return (Routing)GetValue(routingProperty); }
            set { SetValue(routingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for routing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty routingProperty =
            DependencyProperty.Register("routing", typeof(Routing), typeof(DisplayRouting), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayRouting control = d as DisplayRouting;

            if (control == null)
                return;

            control.DataContext = control.routing;
        }

        public DisplayRouting()
        {
            InitializeComponent();
        }
    }
}
