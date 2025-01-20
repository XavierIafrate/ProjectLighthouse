using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLMOItems : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(DisplayLMOItems), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayLMOItems control) return;

            control.CycleTimeNotPresentIndicator.Visibility = control.Item.CycleTime == 0 && control.Item.QuantityMade > 0
                ? Visibility.Visible : Visibility.Collapsed;

            string colourForeground = "Red";
            string colourBackground = "RedFaded";

            if(control.Item.RequiredQuantity > 0 && control.Item.QuantityDelivered >= control.Item.RequiredQuantity)
            {
                colourBackground = "GreenFaded";
                colourForeground = "Green";
            }

            Brush fg = (Brush)Application.Current.Resources[colourForeground];
            Brush bg = (Brush)Application.Current.Resources[colourBackground];

            control.RequirementBadge.Background = bg;
            control.RequirementBadge.BorderBrush = fg;

            control.CustomerRequirementTitleTextBlock.Foreground = fg;
            control.QuantityRequiredTextBlock.Foreground = fg;
            control.DateRequiredTextBlock.Foreground = fg;
        }

        public DisplayLMOItems()
        {
            InitializeComponent();
        }

        private void ClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Item.Gtin);
        }
    }
}
