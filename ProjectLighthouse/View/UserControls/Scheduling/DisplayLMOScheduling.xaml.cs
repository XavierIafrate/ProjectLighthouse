﻿using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.View.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLMOScheduling : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public LatheManufactureOrder orderObject
        {
            get { return (LatheManufactureOrder)GetValue(orderObjectProperty); }
            set { SetValue(orderObjectProperty, value); }
        }

        public static readonly DependencyProperty orderObjectProperty =
            DependencyProperty.Register("orderObject", typeof(LatheManufactureOrder), typeof(DisplayLMOScheduling), new PropertyMetadata(null, SetValues));


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DisplayLMOScheduling control)
            {
                if (control.orderObject == null)
                {
                    return;
                }

                control.DataContext = control.orderObject;

                switch (control.orderObject.State)
                {
                    case OrderState.Problem:
                        control.bg.Background = (Brush)Application.Current.Resources["Red"];
                        control.statusBadgeText.Background = (Brush)Application.Current.Resources["Red"];
                        break;
                    case OrderState.Ready:
                        control.bg.Background = (Brush)Application.Current.Resources["Green"];
                        control.statusBadgeText.Background = (Brush)Application.Current.Resources["Green"];
                        break;
                    case OrderState.Prepared:
                        control.bg.Background = (Brush)Application.Current.Resources["Green"];
                        control.statusBadgeText.Background = (Brush)Application.Current.Resources["Green"];
                        break;
                    case OrderState.Running:
                        control.bg.Background = (Brush)Application.Current.Resources["Blue"];
                        control.statusBadgeText.Background = (Brush)Application.Current.Resources["Blue"];
                        break;
                    default:
                        control.bg.Background = (Brush)Application.Current.Resources["OnBackground"];
                        control.statusBadgeText.Background = (Brush)Application.Current.Resources["Background"];
                        break;
                }
                control.orderItems.ItemsSource = control.orderObject.OrderItems;

                control.LargeDiameterIndicator.Visibility = control.orderObject.OrderItems.First().MajorDiameter > 20
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                control.editButton.Visibility = App.CurrentUser.Role >= UserRole.Scheduling
                    ? Visibility.Visible
                    : Visibility.Collapsed;


                control.WarningBanner.Visibility = Visibility.Collapsed;
                int secondsBudgeted = 0;

                DateTime startDate = control.orderObject.StartDate;
                if (startDate == DateTime.MinValue)
                {
                    return;
                }
                List<LatheManufactureOrderItem> orderedItems = control.orderObject.OrderItems.OrderBy(x => x.DateRequired).ToList();

                for (int i = 0; i < orderedItems.Count; i++)
                {
                    LatheManufactureOrderItem item = orderedItems[i];
                    if (item.RequiredQuantity == 0)
                    {
                        continue;
                    }

                    int cycleTimeToUse = item.CycleTime == 0 ? item.PlannedCycleTime() : item.CycleTime;

                    int secondsToRequirement = (item.RequiredQuantity - item.QuantityDelivered) * cycleTimeToUse;
                    int diff = (int)(item.DateRequired.Date - startDate.Date.AddSeconds(secondsBudgeted)).TotalSeconds;

                    if (item.DateRequired < DateTime.Today && item.RequiredQuantity > item.QuantityDelivered)
                    {
                        control.WarningBanner.Visibility = Visibility.Visible;
                        control.WarningText.Text = "The delivery target has been missed.";
                        break;
                    }
                    else if (diff < secondsToRequirement && secondsBudgeted > 0)
                    {
                        control.WarningBanner.Visibility = Visibility.Visible;
                        control.WarningText.Text = "Highly likely delivery target will be missed.";
                        break;
                    }
                    else if (diff * 0.9 < secondsToRequirement || diff <= 86400 && secondsBudgeted > 0)
                    {
                        control.WarningBanner.Visibility = Visibility.Visible;
                        control.WarningText.Text = "At risk of missing delivery target.";
                    }
                    else
                    {
                        control.WarningBanner.Visibility = Visibility.Collapsed;
                    }

                    secondsBudgeted += secondsToRequirement;
                }
            }
        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetDateWindow dateWindow = new(orderObject) { Owner = App.MainViewModel.MainWindow };
            dateWindow.ShowDialog();

            if (dateWindow.SaveExit)
            {
                orderObject.UpdateStartDate(dateWindow.SelectedDate, dateWindow.AllocatedMachine);
                orderObject.NotifyEditMade();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainCanvas.ColumnDefinitions[2].ActualWidth >= 470)
            {
                Grid.SetColumn(OrderItemsSubControl, 2);
                Grid.SetRow(OrderItemsSubControl, 1);
            }
            else
            {
                Grid.SetColumn(OrderItemsSubControl, 0);
                Grid.SetRow(OrderItemsSubControl, 2);
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            double width = this.mainGrid.ActualWidth;
            double height = this.mainGrid.ActualHeight;

            CopyButton.Visibility = Visibility.Hidden;

            RenderTargetBitmap bmpCopied = new((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new();

            using (DrawingContext dc = dv.RenderOpen())
            {
                Rect rect = new(new Point(), new Size(width, height));
                VisualBrush vb = new(this.mainGrid);
                dc.DrawRectangle(Brushes.White, null, rect);
                dc.DrawRectangle(vb, null, rect);
            }

            bmpCopied.Render(dv);
            try
            {
                Clipboard.SetImage(bmpCopied);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
            }
            CopyButton.Visibility = Visibility.Visible;
        }
    }
}