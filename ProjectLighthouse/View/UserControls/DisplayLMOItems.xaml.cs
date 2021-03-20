using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

                if ((e.NewValue as LatheManufactureOrderItem).IsSpecialPart)
                {
                    control.specialFlag.Visibility = Visibility.Visible;
                }
                else
                {
                    control.specialFlag.Visibility = Visibility.Collapsed;
                }

                if ( control.LatheManufactureOrderItem.RequiredQuantity == 0)
                {
                    control.daterequired.Visibility = Visibility.Collapsed;
                    control.qtyrequired.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.daterequired.Visibility = Visibility.Visible;
                    control.qtyrequired.Visibility = Visibility.Visible;
                }

                if(control.LatheManufactureOrderItem.QuantityMade >= control.LatheManufactureOrderItem.RequiredQuantity)
                {
                    control.qtyrequired.Foreground = (Brush)Application.Current.Resources["colGood"];
                }
                else
                {
                    control.qtyrequired.Foreground = Brushes.Black;
                }

                if (control.LatheManufactureOrderItem.QuantityMade >= control.LatheManufactureOrderItem.TargetQuantity)
                {
                    control.productText.Foreground = (Brush)Application.Current.Resources["colGood"];
                }
                else
                {
                    control.productText.Foreground = Brushes.Black;
                }

            }
        }

        public DisplayLMOItems()
        {
            InitializeComponent();
        }
    }
}
