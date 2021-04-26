using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for LMOConstructionDisplayLMOItems.xaml
    /// </summary>
    public partial class LMOConstructionDisplayLMOItems : UserControl
    {



        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(LMOConstructionDisplayLMOItems), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LMOConstructionDisplayLMOItems control = d as LMOConstructionDisplayLMOItems;

            if (control != null)
            {
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
        }

        public LMOConstructionDisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
