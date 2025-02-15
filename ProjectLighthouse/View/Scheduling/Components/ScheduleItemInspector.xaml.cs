﻿using ProjectLighthouse.Model.Administration;
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



        public DeleteMaintenanceEventCommand DeleteMaintenanceEventCommand
        {
            get { return (DeleteMaintenanceEventCommand)GetValue(DeleteMaintenanceEventCommandProperty); }
            set { SetValue(DeleteMaintenanceEventCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMaintenanceEventCommandProperty =
            DependencyProperty.Register("DeleteMaintenanceEventCommand", typeof(DeleteMaintenanceEventCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null));



        private static void SetRescheduleCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;
            if (control.Item is null) return;

            control.SetRescheduleVis();
        }

        private void SetRescheduleVis()
        {
            RescheduleGrid.Visibility = RescheduleCommand != null  
                                     && Item != null  
                                     && App.CurrentUser.Role >= Model.Administration.UserRole.Scheduling
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

            if (control.Item is GeneralManufactureOrder generalOrder)
            {
                control.ShowGeneralOrder(generalOrder);
                return;
            }

        }

        private void ShowGeneralOrder(GeneralManufactureOrder generalOrder)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Collapsed;
            this.maintenanceGrid.Visibility = Visibility.Collapsed;
            this.generalOrderGrid.Visibility = Visibility.Visible;

            this.ItemNameTextBlock.Text = generalOrder.Name;
            this.ProductNameTextBlock.Text = generalOrder.Item.Name;
            this.ProductDescTextBlock.Text = generalOrder.Item.Description;

            this.FinishedQuantityTextBlock.Text = $"Finished Quantity: {generalOrder.FinishedQuantity:#,##0}";
            this.RequiredQuantityTextBlock.Text = $"Required Quantity: {generalOrder.RequiredQuantity:#,##0}";
            this.GeneralOrderEstimatedRuntimeTextBlock.Text = $"{new intToTimespanString().Convert(generalOrder.TimeToComplete, typeof(string), null, new CultureInfo("en-GB")) ?? "N/A"}";
            this.GeneralOrderStartDateTextBlock.Text = $"{generalOrder.StartDate:dd/MM/yy HH:mm}";
        }

        private void ShowService(MachineService service)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Collapsed;
            this.generalOrderGrid.Visibility = Visibility.Collapsed;
            this.maintenanceGrid.Visibility = Visibility.Visible;

            this.ItemNameTextBlock.Text = service.Name;
        }

        private void ShowOrder(LatheManufactureOrder order)
        {
            this.NothingSelectedTextBlock.Visibility = Visibility.Collapsed;
            this.metaGrid.Visibility = Visibility.Visible;
            this.orderGrid.Visibility = Visibility.Visible;
            this.maintenanceGrid.Visibility = Visibility.Collapsed;
            this.generalOrderGrid.Visibility = Visibility.Collapsed;

            this.ItemNameTextBlock.Text = order.Name;

            this.SettingStartTextBlock.Text = $"{order.GetSettingStartDateTime():dd/MM/yy HH:mm}";
            this.SettingTimeTextBlock.Text = $"{order.TimeToSet}h";
            this.StartDateTextBlock.Text = $"{order.StartDate:dd/MM/yy HH:mm}";

            this.BarStockDisplay.Bar = order.Bar;

            this.OrderItemsItemsControl.ItemsSource = order.OrderItems;
            this.MachineRequirementsItemsControl.ItemsSource = order.RequiredFeaturesList;

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
            this.generalOrderGrid.Visibility = Visibility.Collapsed;
        }

        public ScheduleItemInspector()
        {
            InitializeComponent();

            this.DateTimePicker.DateChanged += RescheduleItem;
            this.CancelServiceButton.IsEnabled = App.CurrentUser.HasPermission(Model.Core.PermissionType.ConfigureMaintenance);
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

        private void CancelServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Item is not MachineService service) return;
            if (this.DeleteMaintenanceEventCommand is null) return;

            DeleteMaintenanceEventCommand?.Execute(service);
        }
    }
}
