using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;

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
