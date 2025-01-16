using ProjectLighthouse.Model.Scheduling;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayOrderScheduleInformation : UserControl
    {
        public ScheduleItem Item
        {
            get { return (ScheduleItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ScheduleItem), typeof(DisplayOrderScheduleInformation), new PropertyMetadata(null));


        public DisplayOrderScheduleInformation()
        {
            InitializeComponent();
        }
    }
}
