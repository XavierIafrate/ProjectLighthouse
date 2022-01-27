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
                        int numberNeeded = control.Product.QuantityInStock + control.Product.QuantityOnPO - control.Product.QuantityOnSO;
                        numberNeeded *= -1;
                        int differential = (numberNeeded + control.Product.QuantityOnPO) - control.Product.QuantityOnOrder;
                        if (differential == 0) // Order has already been created/updated
                        {
                            control.CouldBeAddedFlag.Text = $"Order {control.Product.OrderReference} has matching requirements";
                        }
                        else if (differential > 0)
                        {
                            control.CouldBeAddedFlag.Text = $"{control.Product.OrderReference} need requirements increased";
                        }
                        else
                        {
                            control.CouldBeAddedFlag.Text = $"{control.Product.OrderReference} will cover this";
                        }
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
