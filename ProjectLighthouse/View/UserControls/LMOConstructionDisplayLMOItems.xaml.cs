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
            LMOConstructionDisplayLMOItems control = d as LMOConstructionDisplayLMOItems;

            if (control == null)
                return;
            control.DataContext = control.Item;

            if (control.Item.RequiredQuantity == 0)
            {
                control.requiredDate.Visibility = Visibility.Hidden;
                control.requiredQuantity.Visibility = Visibility.Hidden;
            }
            else
            {
                control.requiredDate.Visibility = Visibility.Visible;
                control.requiredQuantity.Visibility = Visibility.Visible;
            }
        }

        public LMOConstructionDisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
