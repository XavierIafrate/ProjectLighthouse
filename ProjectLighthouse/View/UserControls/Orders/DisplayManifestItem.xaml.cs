using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayManifestItem : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }
        public static readonly DependencyProperty ItemProperty =
                    DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(DisplayManifestItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayManifestItem control) return;

            control.DataContext = control.Item;

            control.RequiredText.Visibility = control.Item.RequiredQuantity == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public DisplayManifestItem()
        {
            InitializeComponent();
        }
    }
}
