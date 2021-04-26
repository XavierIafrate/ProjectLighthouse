using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayLMOItems.xaml
    /// </summary>
    public partial class DisplayLMOItems : UserControl
    {


        public LatheManufactureOrderItem LatheManufactureOrderItem
        {
            get { return (LatheManufactureOrderItem)GetValue(LatheManufactureOrderItemProperty); }
            set { SetValue(LatheManufactureOrderItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatheManufactureOrderItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatheManufactureOrderItemProperty =
            DependencyProperty.Register("LatheManufactureOrderItem", typeof(LatheManufactureOrderItem), typeof(DisplayLMOItems), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLMOItems control = d as DisplayLMOItems;

            if (control != null)
            {
                control.DataContext = control.LatheManufactureOrderItem;

                control.specialFlag.Visibility = (control.LatheManufactureOrderItem.IsSpecialPart) ? Visibility.Visible : Visibility.Collapsed;

                control.requirements.Visibility = (control.LatheManufactureOrderItem.RequiredQuantity == 0) ? Visibility.Collapsed : Visibility.Visible;

                control.requirementsBadge.Fill = (control.LatheManufactureOrderItem.QuantityMade >= control.LatheManufactureOrderItem.RequiredQuantity) ?
                    (Brush)Application.Current.Resources["materialPrimaryGreen"] : (Brush)Application.Current.Resources["materialError"];
            }
        }

        public DisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
