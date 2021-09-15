using ProjectLighthouse.Model.Assembly;
using System.Windows;
using System.Windows.Controls;

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
            if (d is not DisplayRouting control)
                return;

            control.DataContext = control.routing;
        }

        public DisplayRouting()
        {
            InitializeComponent();
        }
    }
}
