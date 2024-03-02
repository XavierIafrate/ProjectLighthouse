using ProjectLighthouse.Model.Scheduling;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayScheduleItem : UserControl
    {
        public ScheduleItem Order
        {
            get { return (ScheduleItem)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(ScheduleItem), typeof(DisplayScheduleItem), new PropertyMetadata(null));


        public DisplayScheduleItem()
        {
            InitializeComponent();
        }
    }
}
