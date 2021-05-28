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
    /// Interaction logic for DisplayAssemblyManufactureOrder.xaml
    /// </summary>
    public partial class DisplayAssemblyManufactureOrder : UserControl
    {
        public AssemblyManufactureOrder Order
        {
            get { return (AssemblyManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(AssemblyManufactureOrder), typeof(DisplayAssemblyManufactureOrder), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayAssemblyManufactureOrder control = d as DisplayAssemblyManufactureOrder;

            if (control == null)
                return;

            control.DataContext = control.Order;

            control.OldInfo.Visibility = (control.Order.ModifiedAt.AddDays(2) < DateTime.Now
                && control.Order.Status == "Problem") ? Visibility.Visible : Visibility.Hidden;

            switch (control.Order.Status)
            {
                case "Ready":
                    control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                    break;
                case "Problem":
                    control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialError"];
                    break;
                case "Running":
                    control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialPrimaryBlueVar"];
                    break;
                case "Complete":
                    control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialOnBackground"];
                    break;
            }
        }

        public DisplayAssemblyManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
