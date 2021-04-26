using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayProduct.xaml
    /// </summary>
    public partial class DisplayProduct : UserControl
    {
        public TurnedProduct TurnedProduct
        {
            get { return (TurnedProduct)GetValue(TurnedProductProperty); }
            set { SetValue(TurnedProductProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TurnedProduct.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TurnedProductProperty =
            DependencyProperty.Register("TurnedProduct", typeof(TurnedProduct), typeof(DisplayProduct), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayProduct control = d as DisplayProduct;
            if (control != null)
            {
                control.DataContext = control.TurnedProduct;
                if (!control.TurnedProduct.canBeManufactured())
                {
                    control.productText.Foreground = Brushes.Red;
                }
                if (String.IsNullOrEmpty(control.TurnedProduct.Material))
                {
                    control.materialBadge.Visibility = Visibility.Hidden;
                }
                else
                {
                    control.materialBadge.Visibility = Visibility.Visible;
                }
                switch (control.TurnedProduct.Material)
                {
                    case "A2":
                        control.materialBadge.Fill = (Brush)Application.Current.Resources["materialOnBackground"];
                        break;
                    case "A4":
                        control.materialBadge.Fill = (Brush)Application.Current.Resources["materialPrimary"];
                        break;
                    case "Ti":
                        control.materialBadge.Fill = (Brush)Application.Current.Resources["materialError"];
                        break;
                    case "EN1A":
                        control.materialBadge.Fill = (Brush)Application.Current.Resources["materialPrimaryGreenVar"];
                        break;
                    default:
                        control.materialBadge.Fill = Brushes.Black;
                        break;
                }
            }

        }

        public DisplayProduct()
        {
            InitializeComponent();
        }
    }
}
