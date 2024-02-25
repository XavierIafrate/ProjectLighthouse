using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.ValueConverters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class ScheduleItemInspector : UserControl
    {
        public ScheduleItem? Item
        {
            get { return (ScheduleItem?)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ScheduleItem), typeof(ScheduleItemInspector), new PropertyMetadata(null, SetValues));

        public RescheduleItemCommand RescheduleCommand
        {
            get { return (RescheduleItemCommand)GetValue(RescheduleCommandProperty); }
            set { SetValue(RescheduleCommandProperty, value); }
        }

        public static readonly DependencyProperty RescheduleCommandProperty =
            DependencyProperty.Register("RescheduleCommand", typeof(RescheduleItemCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null, SetRescheduleCommand));

        private static void SetRescheduleCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;
            if (control.Item is null) return;

            control.SetRescheduleVis();
        }

        private void SetRescheduleVis()
        {
            RescheduleGrid.Visibility = RescheduleCommand != null  && Item != null 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;
            control.SetRescheduleVis();

            if (control.Item is null)
            {
                control.SetNothingSelected();
                return;
            }

            control.DateTimePicker.DateTime = control.Item.StartDate;

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

            this.ItemNameTextBlock.Text = service.Name;
        }

        private void ShowOrder(LatheManufactureOrder order)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Visible;
            this.maintenanceGrid.Visibility = Visibility.Collapsed;

            this.ItemNameTextBlock.Text = order.Name;

            this.SettingStartTextBlock.Text = $"{order.GetSettingStartDateTime():dd/MM/yy HH:mm}";
            this.SettingTimeTextBlock.Text = $"{order.TimeToSet}h";
            this.StartDateTextBlock.Text = $"{order.StartDate:dd/MM/yy HH:mm}";

            this.BarStockDisplay.Bar = order.Bar;

            //if (order.Bar.IsHexagon)
            //{
            //    this.RoundProfileSymbol.Visibility = Visibility.Collapsed;
            //    this.HexagonProfileSymbol.Visibility = Visibility.Visible;
            //    this.BarSizeTextBlock.Text = $"{order.Bar.Size}mm A/F";
            //}
            //else
            //{
            //    this.RoundProfileSymbol.Visibility = Visibility.Visible;
            //    this.HexagonProfileSymbol.Visibility = Visibility.Collapsed;
            //    this.BarSizeTextBlock.Text = $"{order.Bar.Size}mm";
            //}

            //this.BarIdTextBlock.Text = order.Bar.Id;
            //this.MaterialInfoTextBlock.Text = order.Bar.MaterialData.ToString();

            this.OrderItemsItemsControl.ItemsSource = order.OrderItems;
            this.OrderBreakdownsItemsControl.ItemsSource = order.Breakdowns;

            this.EstimatedRuntimeTextBlock.Text = $"{new intToTimespanString().Convert(order.TimeToComplete, typeof(string), null, new CultureInfo("en-GB")) ?? "N/A"}";

            if (order.GetStartDeadline() == DateTime.MaxValue)
            {
                this.DeadlineTextBlock.Text = "None";
            }
            else
            {
                this.DeadlineTextBlock.Text = $"{order.GetStartDeadline():dd/MM/yy}";
            }

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

            this.DateTimePicker.DateChanged += RescheduleItem;
        }

        private void RescheduleItem(object sender, EventArgs e)
        {
            if (this.Item is null) return;
            if (this.RescheduleCommand is null) return;
            DateTime desiredDate = this.DateTimePicker.DateTime;
            if (desiredDate == this.Item.StartDate) return;

            RescheduleInformation data = new(this.Item, this.Item.AllocatedMachine, desiredDate);

            RescheduleCommand?.Execute(data);
        }
    }
}
