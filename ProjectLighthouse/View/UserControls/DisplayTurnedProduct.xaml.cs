using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayTurnedProduct.xaml
    /// </summary>
    public partial class DisplayTurnedProduct : UserControl
    {


        public TurnedProduct Product
        {
            get { return (TurnedProduct)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(TurnedProduct), typeof(DisplayTurnedProduct), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayTurnedProduct control)
                return;

            if (control.Product != null)
            {
                control.ProductName.Text = control.Product.ProductName;
                control.MaterialTextBlock.Text = control.Product.Material;
                control.HotFlag.Visibility = control.Product.QuantitySold > 8000
                    ? Visibility.Visible
                    : Visibility.Hidden;

                switch (control.Product.Material)
                {
                    case "A2":
                        control.Badge.Fill = (Brush)App.Current.Resources["materialPrimaryBlue"];
                        break;
                    case "A4":
                        control.Badge.Fill = (Brush)App.Current.Resources["materialPrimary"];
                        break;
                    case "EN1A":
                        control.Badge.Fill = (Brush)App.Current.Resources["materialPrimaryGreen"];
                        break;
                    case "TI":
                        control.Badge.Fill = (Brush)App.Current.Resources["materialError"];
                        break;
                }

                if (!string.IsNullOrEmpty(control.Product.OrderReference))
                {
                    if (control.Product.IsAlreadyOnOrder)
                    {
                        control.CouldBeAddedFlag.Text = $"{control.Product.QuantityOnOrder:#,##0}pcs on order {control.Product.OrderReference}";
                        control.CouldBeAddedFlag.Foreground = (Brush)App.Current.Resources["materialPrimaryGreen"];
                    }
                    else
                    {
                        control.CouldBeAddedFlag.Text = $"could add to {control.Product.OrderReference}";
                        control.CouldBeAddedFlag.Foreground = (Brush)App.Current.Resources["materialPrimaryBlue"];
                    }

                    control.CouldBeAddedFlag.Visibility = Visibility.Visible;
                }
                else
                {
                    control.RecentlyDeclined.Visibility = control.Product.RecentlyDeclined
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    control.CouldBeAddedFlag.Visibility = Visibility.Collapsed;
                }
            }
        }

        public DisplayTurnedProduct()
        {
            InitializeComponent();
        }
    }
}
