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
    /// Interaction logic for DisplayLatheManufactureOrder.xaml
    /// </summary>
    public partial class DisplayLatheManufactureOrder : UserControl
    {



        public LatheManufactureOrder LatheManufactureOrder
        {
            get { return (LatheManufactureOrder)GetValue(LatheManufactureOrderProperty); }
            set { SetValue(LatheManufactureOrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatheManufactureOrder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatheManufactureOrderProperty =
            DependencyProperty.Register("LatheManufactureOrder", typeof(LatheManufactureOrder), typeof(DisplayLatheManufactureOrder), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLatheManufactureOrder control = d as DisplayLatheManufactureOrder;

            if (control != null)
            {
                control.DataContext = control.LatheManufactureOrder;
                control.BadgeText.Text = control.LatheManufactureOrder.Status;
                switch (control.LatheManufactureOrder.Status)
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
            
        }

        public DisplayLatheManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
