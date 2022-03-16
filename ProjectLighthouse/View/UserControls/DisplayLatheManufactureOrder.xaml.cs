using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLatheManufactureOrder : UserControl
    {
        public LatheManufactureOrder LatheManufactureOrder
        {
            get { return (LatheManufactureOrder)GetValue(LatheManufactureOrderProperty); }
            set { SetValue(LatheManufactureOrderProperty, value); }
        }

        public static readonly DependencyProperty LatheManufactureOrderProperty =
            DependencyProperty.Register("LatheManufactureOrder", typeof(LatheManufactureOrder), typeof(DisplayLatheManufactureOrder), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayLatheManufactureOrder control)
                return;

            control.DataContext = control.LatheManufactureOrder;

            control.RecentUpdateFlag.Visibility = (control.LatheManufactureOrder.ModifiedAt.AddHours(5) > System.DateTime.Now)
                && control.LatheManufactureOrder.State < OrderState.Running
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (control.LatheManufactureOrder.State != OrderState.Running && control.LatheManufactureOrder.State != OrderState.Problem)
            {
                control.badgeBackground.Visibility = Visibility.Collapsed;
            }
            
        }

        public DisplayLatheManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
