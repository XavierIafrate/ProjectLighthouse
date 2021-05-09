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

                control.requirementsBadge.Fill = (control.LatheManufactureOrderItem.QuantityMade >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["materialPrimaryGreen"] : (Brush)Application.Current.Resources["materialError"];

                if (control.LatheManufactureOrderItem.QuantityMade >= control.LatheManufactureOrderItem.TargetQuantity) // if done
                {
                    control.productText.Foreground = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                    control.bgRect.Stroke = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                    control.bgRect.StrokeThickness = 2;
                }
                else
                {
                    control.productText.Foreground = (Brush)Application.Current.Resources["materialOnBackground"];
                    control.bgRect.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f0f0f0"));
                    control.bgRect.StrokeThickness = 1;
                }
            }
        }

        public DisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
