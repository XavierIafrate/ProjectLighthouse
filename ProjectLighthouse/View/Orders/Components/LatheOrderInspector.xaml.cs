using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class LatheOrderInspector : UserControl
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(LatheOrderInspector), new PropertyMetadata(null, SetOrder));

        private static void SetOrder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LatheOrderInspector control) return;
            if (control.Order is null) return;
            if (control.Order.State < OrderState.Running)
            {
                control.OrderTabControl.SelectedIndex = 0;
            }
            else
            {
                control.OrderTabControl.SelectedIndex = 2;
            }
        }

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(LatheOrderInspector), new PropertyMetadata(false));


        public List<User> ProductionStaff
        {
            get { return (List<User>)GetValue(ProductionStaffProperty); }
            set { SetValue(ProductionStaffProperty, value); }
        }

        public static readonly DependencyProperty ProductionStaffProperty =
            DependencyProperty.Register("ProductionStaff", typeof(List<User>), typeof(LatheOrderInspector), new PropertyMetadata(null));

        public bool HasConfigPermission { get; set; } = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditOrder);

        public LatheOrderInspector()
        {
            InitializeComponent();
        }
    }
}
