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

        public static readonly DependencyProperty TurnedProductProperty =
            DependencyProperty.Register("TurnedProduct", typeof(TurnedProduct), typeof(DisplayProduct), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayProduct control = d as DisplayProduct;
            if (control == null)
                return;
            
            control.DataContext = control.TurnedProduct;

            if (!control.TurnedProduct.canBeManufactured())
                control.productText.Foreground = Brushes.Red;
                
            control.materialBadge.Visibility = String.IsNullOrEmpty(control.TurnedProduct.Material) ? Visibility.Hidden : Visibility.Visible;

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

        public DisplayProduct()
        {
            InitializeComponent();
        }
    }
}
