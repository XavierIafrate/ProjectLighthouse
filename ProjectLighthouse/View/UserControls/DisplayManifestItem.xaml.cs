using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayManifestItem : UserControl
    {


        public Model.LatheManufactureOrderItem Item
        {
            get { return (Model.LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(Model.LatheManufactureOrderItem), typeof(DisplayManifestItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayManifestItem control)
            {
                return;
            }

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
