using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class LMOConstructionDisplayLMOItems : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(LMOConstructionDisplayLMOItems), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LMOConstructionDisplayLMOItems control)
            {
                return;
            }

            if (control == null)
            {
                return;
            }

            control.DataContext = control.Item;

            control.customerRequirementBadge.Visibility = control.Item.RequiredQuantity == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        public LMOConstructionDisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
