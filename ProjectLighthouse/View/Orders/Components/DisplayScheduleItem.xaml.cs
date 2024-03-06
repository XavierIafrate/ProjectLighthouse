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

            if (App.CurrentUser.Role == Model.Administration.UserRole.Purchasing)
            {
                PoTextBlock.Visibility = Visibility.Visible;
                assignmentWrapper.Visibility = Visibility.Collapsed;
            }
            else
            {
                PoTextBlock.Visibility = Visibility.Collapsed;
                assignmentWrapper.Visibility = Visibility.Visible;
            }
        }
    }
}
