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

        public CompleteOrder orderObject
        {
            get { return (CompleteOrder)GetValue(orderObjectProperty); }
            set { SetValue(orderObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty orderObjectProperty =
            DependencyProperty.Register("orderObject", typeof(CompleteOrder), typeof(DisplayLMOScheduling), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLMOScheduling control = d as DisplayLMOScheduling;

            if (control != null)
            {
                control.DataContext = control.orderObject;

                switch (control.orderObject.Order.Status)
                {
                    case "Ready":
                        control.bg.Fill = (Brush)App.Current.Resources["colGood"];
                        break;
                    case "Awaiting scheduling":
                        control.bg.Fill = (Brush)App.Current.Resources["colAdvise"];
                        break;
                    case "Running":
                        control.bg.Fill = (Brush)App.Current.Resources["colNeutral"];
                        break;
                    case "Problem":
                        control.bg.Fill = (Brush)App.Current.Resources["colCritical"];
                        break;
                    default:
                        control.bg.Fill = Brushes.DodgerBlue;
                        break;
                }
            }
        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }
    }
}
