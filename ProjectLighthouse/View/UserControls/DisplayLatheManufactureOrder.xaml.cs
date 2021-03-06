using ProjectLighthouse.Model;
using System;
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

            control.PoTextBlock.Visibility = (App.CurrentUser.Role == UserRole.Administrator || App.CurrentUser.Role == UserRole.Purchasing)
                ?Visibility.Visible
                :Visibility.Collapsed;

            bool needsUpdate = control.LatheManufactureOrder.ModifiedAt.AddDays(3) < DateTime.Now
                   && control.LatheManufactureOrder.State == OrderState.Problem
                   && control.LatheManufactureOrder.CreatedAt.AddDays(3) < DateTime.Now;

            needsUpdate = needsUpdate && !(control.LatheManufactureOrder.BarIsVerified && control.LatheManufactureOrder.HasProgram && control.LatheManufactureOrder.IsReady);


            control.BadgeText.Visibility = control.LatheManufactureOrder.IsClosed || needsUpdate ? Visibility.Collapsed : Visibility.Visible;
        }

        public DisplayLatheManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
