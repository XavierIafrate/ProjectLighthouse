using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.ValueConverters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            if (control.LatheManufactureOrder == null) return;

            control.OrderNameTextBlock.Text = control.LatheManufactureOrder.Name;
            control.AllocatedMachineTextBlock.Text = control.LatheManufactureOrder.AllocatedMachine;
            control.PoTextBlock.Text = control.LatheManufactureOrder.POReference;
            control.AssignmentTextBlock.Text = control.LatheManufactureOrder.AssignedTo;

            control.RecentUpdateFlag.Visibility = (control.LatheManufactureOrder.ModifiedAt.AddHours(5) > DateTime.Now)
                && control.LatheManufactureOrder.State < OrderState.Running
                ? Visibility.Visible
                : Visibility.Collapsed;

            bool needsUpdate = control.LatheManufactureOrder.ModifiedAt.AddDays(5) < DateTime.Now
                   && control.LatheManufactureOrder.State == OrderState.Problem
                   && control.LatheManufactureOrder.CreatedAt.AddDays(5) < DateTime.Now;

            control.StaleBadge.Visibility =
                       control.LatheManufactureOrder.ModifiedAt.AddDays(5) < DateTime.Now
                    && control.LatheManufactureOrder.State == OrderState.Problem
                    && control.LatheManufactureOrder.CreatedAt.AddDays(5) < DateTime.Now
                    && control.LatheManufactureOrder.State == OrderState.Problem
                            ? Visibility.Visible
                            : Visibility.Collapsed;

            control.StatusBadgeText.Text = control.LatheManufactureOrder.State.ToString();
            control.StatusBadge.Visibility = control.LatheManufactureOrder.IsClosed || needsUpdate ? Visibility.Collapsed : Visibility.Visible;
            OrderStateToBrush converter = new();
            control.StatusBadge.Background = converter.Convert(control.LatheManufactureOrder.State, null, "faded", null) as Brush;
            control.StatusBadgeText.Foreground = converter.Convert(control.LatheManufactureOrder.State, null, null, null) as Brush;
            control.ClosedBadge.Visibility = control.LatheManufactureOrder.IsClosed ? Visibility.Visible : Visibility.Collapsed;

            control.PoTextBlock.Visibility = App.CurrentUser.Role is Model.Administration.UserRole.Purchasing
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.clippy.Visibility = control.PoTextBlock.Visibility == Visibility.Visible || control.LatheManufactureOrder.AssignedTo is null
                ? Visibility.Collapsed
                : Visibility.Visible;

            control.AssignmentTextBlock.Visibility = control.PoTextBlock.Visibility == Visibility.Visible || control.LatheManufactureOrder.AssignedTo is null
                ? Visibility.Collapsed
                : Visibility.Visible;

        }

        public DisplayLatheManufactureOrder()
        {
            InitializeComponent();
        }
    }
}
