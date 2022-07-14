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
    /// Interaction logic for LMOQuantityDisplay.xaml
    /// </summary>
    public partial class LMOQuantityDisplay : UserControl
    {



        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(LMOQuantityDisplay), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LMOQuantityDisplay control) return;
            control.DataContext = control.Item;

            control.progressBar.Foreground = (Brush)Application.Current.Resources[control.Item.QuantityDelivered >= control.Item.TargetQuantity ? "Green" : "Purple"];
            control.targetMarker.Fill = (Brush)Application.Current.Resources[control.Item.QuantityDelivered >= control.Item.RequiredQuantity ? "Green" : "Red"];
            control.targetMarker.Visibility = control.Item.RequiredQuantity > 0 ? Visibility.Visible : Visibility.Hidden;
            control.markerRow.Height = control.Item.QuantityMade > 0 ? new(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            control.progressBarRow.Height = control.Item.QuantityMade > 0 ? new(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            control.delivered.Visibility = control.Item.QuantityMade > 0 ? Visibility.Visible : Visibility.Collapsed;

            control.made.Visibility = control.Item.QuantityMade != control.Item.QuantityDelivered ? Visibility.Visible : Visibility.Collapsed;

            double lWidth = (double)control.Item.RequiredQuantity / (double)control.Item.TargetQuantity;
            if (double.IsNaN(lWidth))
            {
                lWidth = 0;
            }
            lWidth *= 100;

            control.lCol.Width = new(lWidth, GridUnitType.Star);
            control.rCol.Width = new(100 - lWidth, GridUnitType.Star);

        }

        public LMOQuantityDisplay()
        {
            InitializeComponent();
        }
    }
}
