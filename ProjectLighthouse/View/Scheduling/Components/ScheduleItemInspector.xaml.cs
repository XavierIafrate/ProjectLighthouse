using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class ScheduleItemInspector : UserControl
    {
        public ScheduleItem? Item
        {
            get { return (ScheduleItem?)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ScheduleItem), typeof(ScheduleItemInspector), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;

            if(control.Item is null)
            {
                control.SetNothingSelected();
                return;
            }

            if (control.Item is LatheManufactureOrder order)
            {
                control.ShowOrder(order);
                return;
            }

            if (control.Item is MachineService service)
            {
                control.ShowService(service);
                return;
            }

        }

        private void ShowService(MachineService service)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Collapsed;
            this.maintenanceGrid.Visibility = Visibility.Visible;

            this.ItemNameTextBlock.Text  = service.Name;
        }

        private void ShowOrder(LatheManufactureOrder order)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Visible;
            this.maintenanceGrid.Visibility = Visibility.Collapsed;

            this.ItemNameTextBlock.Text = order.Name;

            this.SettingStartTextBlock.Text = $"Setting Starts @ {order.GetSettingStartDateTime():dd/MM HH:mm}";
            this.SettingTimeTextBlock.Text = $"Setting Allowance: {order.TimeToSet}h";
            this.StartDateTextBlock.Text = $"Running Starts @ {order.StartDate:dd/MM HH:mm}";

            if(order.Bar.IsHexagon)
            {
                this.RoundProfileSymbol.Visibility = Visibility.Collapsed;
                this.HexagonProfileSymbol.Visibility = Visibility.Visible;
                this.BarSizeTextBlock.Text = $"{order.Bar.Size}mm A/F";
            }
            else
            {
                this.RoundProfileSymbol.Visibility = Visibility.Visible;
                this.HexagonProfileSymbol.Visibility = Visibility.Collapsed;
                this.BarSizeTextBlock.Text = $"{order.Bar.Size}mm";
            }

            this.BarIdTextBlock.Text = order.Bar.Id;
            this.MaterialInfoTextBlock.Text = order.Bar.MaterialData.ToString();

            this.OrderItemsItemsControl.ItemsSource = order.OrderItems;
        }

        private void SetNothingSelected()
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Visible;
            this.metaGrid.Visibility = Visibility.Collapsed;
            this.orderGrid.Visibility = Visibility.Collapsed;
            this.maintenanceGrid.Visibility = Visibility.Collapsed;
        }

        public ScheduleItemInspector()
        {
            InitializeComponent();
        }
    }
}
