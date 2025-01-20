using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayGeneralOrderRequirement : UserControl
    {
        public GeneralManufactureOrder Order
        {
            get { return (GeneralManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(GeneralManufactureOrder), typeof(DisplayGeneralOrderRequirement), new PropertyMetadata(null, SetUI));

        private static void SetUI(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayGeneralOrderRequirement control) return;
            if (control.Order is null) return;

            string colourForeground = "Red";
            string colourBackground = "RedFaded";

            if (control.Order.RequiredQuantity > 0 && control.Order.DeliveredQuantity >= control.Order.RequiredQuantity)
            {
                colourBackground = "GreenFaded";
                colourForeground = "Green";
            }

            Brush fg = (Brush)Application.Current.Resources[colourForeground];
            Brush bg = (Brush)Application.Current.Resources[colourBackground];

            control.RequirementBadge.Background = bg;
            control.RequirementBadge.BorderBrush = fg;

            control.QuantityRequiredTextBlock.Foreground = fg;
            control.DateRequiredTextBlock.Foreground = fg;
        }

        public DisplayGeneralOrderRequirement()
        {
            InitializeComponent();
        }
    }
}
