using ProjectLighthouse.Model.Orders;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class LatheOrderItemInspector : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(LatheOrderItemInspector), new PropertyMetadata(null));



        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(LatheOrderItemInspector), new PropertyMetadata(false));


        public bool SchedulingPermissions
        {
            get { return (bool)GetValue(SchedulingPermissionsProperty); }
            set { SetValue(SchedulingPermissionsProperty, value); }
        }

        public static readonly DependencyProperty SchedulingPermissionsProperty =
            DependencyProperty.Register("SchedulingPermissions", typeof(bool), typeof(LatheOrderItemInspector), new PropertyMetadata(false));


        public LatheOrderItemInspector()
        {
            InitializeComponent();
        }

        private void RemoveRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            Item.RequiredQuantity = 0;
            Item.DateRequired = DateTime.MinValue;
        }
    }
}
