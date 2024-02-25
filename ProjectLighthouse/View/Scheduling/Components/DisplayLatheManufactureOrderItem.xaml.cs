using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayLatheManufactureOrderItem : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(DisplayLatheManufactureOrderItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayLatheManufactureOrderItem control) return;
            if (control.Item is null) return;

            control.ItemNameTextBlock.Text = control.Item.ProductName;
            control.RequirementTextBlock.Text = $"{control.Item.RequiredQuantity:#,##0} pcs for {control.Item.DateRequired:dd/MM}"; ;
            control.TargetQtyTextBlock.Text = $"{control.Item.TargetQuantity:#,##0} pcs";

            if (control.Item.RequiredQuantity == 0)
            {
                control.RequirementBadge.Visibility = Visibility.Collapsed;
                control.MainGrid.RowDefinitions[1].Height = new(0);
            }
            else
            {
                control.RequirementBadge.Visibility = Visibility.Visible;
                control.MainGrid.RowDefinitions[1].Height = new (1, GridUnitType.Star);
            }
        }

        public DisplayLatheManufactureOrderItem()
        {
            InitializeComponent();
        }
    }
}
