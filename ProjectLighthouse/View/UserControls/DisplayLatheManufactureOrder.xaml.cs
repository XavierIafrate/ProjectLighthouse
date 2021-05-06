using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayLatheManufactureOrder.xaml
    /// </summary>
    public partial class DisplayLatheManufactureOrder : UserControl
    {



        public LatheManufactureOrder LatheManufactureOrder
        {
            get { return (LatheManufactureOrder)GetValue(LatheManufactureOrderProperty); }
            set { SetValue(LatheManufactureOrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatheManufactureOrder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatheManufactureOrderProperty =
            DependencyProperty.Register("LatheManufactureOrder", typeof(LatheManufactureOrder), typeof(DisplayLatheManufactureOrder), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLatheManufactureOrder control = d as DisplayLatheManufactureOrder;

            if (control != null)
            {
                control.DataContext = control.LatheManufactureOrder;
                control.BadgeText.Text = control.LatheManufactureOrder.Status;
                control.OldInfo.Visibility = (control.LatheManufactureOrder.ModifiedAt.AddDays(2) < DateTime.Now 
                    && control.LatheManufactureOrder.Status == "Problem") ? Visibility.Visible : Visibility.Hidden;
                switch (control.LatheManufactureOrder.Status)
                {
                    case "Ready":
                        control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                        break;
                    case "Problem":
                        control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialError"];
                        break;
                    case "Running":
                        control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialPrimaryBlueVar"];
                        break;
                    case "Complete":
                        control.badgeBackground.Fill = (Brush)Application.Current.Resources["materialOnBackground"];
                        break;


                }

            }

        }

        public DisplayLatheManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
