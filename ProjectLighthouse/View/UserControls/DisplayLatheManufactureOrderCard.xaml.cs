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
    /// Interaction logic for DisplayLatheManufactureOrderCard.xaml
    /// </summary>
    public partial class DisplayLatheManufactureOrderCard : UserControl
    {

        public LatheManufactureOrder LatheManufactureOrder
        {
            get { return (LatheManufactureOrder)GetValue(LatheManufactureOrderProperty); }
            set { SetValue(LatheManufactureOrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatheManufactureOrder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatheManufactureOrderProperty =
            DependencyProperty.Register("LatheManufactureOrder", typeof(LatheManufactureOrder), typeof(DisplayLatheManufactureOrderCard), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLatheManufactureOrderCard cardControl = d as DisplayLatheManufactureOrderCard;

            if (cardControl.LatheManufactureOrder != null)
            {
                cardControl.DataContext = cardControl.LatheManufactureOrder;

                if (!String.IsNullOrEmpty(cardControl.LatheManufactureOrder.ModifiedBy))
                {
                    cardControl.ModifiedTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    cardControl.ModifiedTextBlock.Visibility = Visibility.Collapsed;
                }

                if (cardControl.LatheManufactureOrder.IsReady)
                {
                    cardControl.StatusRectangle.Fill = (LinearGradientBrush)Application.Current.Resources["gradGood"];
                }
                else
                {
                    cardControl.StatusRectangle.Fill = (LinearGradientBrush)Application.Current.Resources["gradBad"];
                }
            }
        }

        public DisplayLatheManufactureOrderCard()
        {
            InitializeComponent();
        }
    }
}