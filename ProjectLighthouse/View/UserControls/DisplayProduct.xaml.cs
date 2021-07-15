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

            if (!control.TurnedProduct.CanBeManufactured())
                control.productText.Foreground = Brushes.Red;

            control.materialBadge.Visibility = String.IsNullOrEmpty(control.TurnedProduct.Material) ? Visibility.Hidden : Visibility.Visible;

            control.materialBadge.Fill = control.TurnedProduct.Material switch
            {
                "A2" => (Brush)Application.Current.Resources["materialOnBackground"],
                "A4" => (Brush)Application.Current.Resources["materialPrimary"],
                "Ti" => (Brush)Application.Current.Resources["materialError"],
                "EN1A" => (Brush)Application.Current.Resources["materialPrimaryGreenVar"],
                _ => Brushes.Black,
            };
        }

        public DisplayProduct()
        {
            InitializeComponent();
        }
    }
}
