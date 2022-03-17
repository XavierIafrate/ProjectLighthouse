using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayTurnedProduct : UserControl
    {
        public TurnedProduct Product
        {
            get { return (TurnedProduct)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

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
                        control.Badge.Fill = (Brush)App.Current.Resources["Blue"];
                        control.MaterialTextBlock.Foreground = (Brush)App.Current.Resources["OnBlue"];
                        break;
                    case "A4":
                        control.Badge.Fill = (Brush)App.Current.Resources["Purple"];
                        control.MaterialTextBlock.Foreground = (Brush)App.Current.Resources["OnPurple"];
                        break;
                    case "EN1A":
                        control.Badge.Fill = (Brush)App.Current.Resources["Green"];
                        control.MaterialTextBlock.Foreground = (Brush)App.Current.Resources["OnGreen"];
                        break;
                    case "TI":
                        control.Badge.Fill = (Brush)App.Current.Resources["Red"];
                        control.MaterialTextBlock.Foreground = (Brush)App.Current.Resources["OnRed"];
                        break;
                }

                if (control.Product.IsAlreadyOnOrder)
                {
                    int numberNeeded = -control.Product.FreeStock();


                    if (numberNeeded == 0)
                    {
                        control.CouldBeAddedFlag.Text = $"{control.Product.OrderReference} matches demand";
                    }
                    else if (numberNeeded > 0)
                    {
                        control.CouldBeAddedFlag.Text = $"Append {control.Product.OrderReference}: +{numberNeeded:#,##0} pcs";
                    }
                    else
                    {
                        control.CouldBeAddedFlag.Text = control.Product.LighthouseGuaranteedQuantity == 0
                            ? $"Uncommitted on {control.Product.OrderReference}"
                            : $"{control.Product.OrderReference} guarantees {control.Product.LighthouseGuaranteedQuantity:#,##0}";
                    }

                    control.CouldBeAddedFlag.Foreground = (Brush)App.Current.Resources["Green"];
                    control.CouldBeAddedFlag.Visibility = Visibility.Visible;
                }
                else if (!string.IsNullOrEmpty(control.Product.OrderReference))
                {
                    control.CouldBeAddedFlag.Text = $"Compatible with {control.Product.OrderReference}";
                    control.CouldBeAddedFlag.Foreground = (Brush)App.Current.Resources["Blue"];
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
