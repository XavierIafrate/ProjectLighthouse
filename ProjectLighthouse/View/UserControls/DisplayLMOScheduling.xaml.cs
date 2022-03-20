using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLMOScheduling : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //public CompleteOrder completeOrder { get; set; }

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
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["Red"];
                        break;
                    case OrderState.Ready:
                        control.bg.Background = (Brush)Application.Current.Resources["Green"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["Green"];
                        break;
                    case OrderState.Prepared:
                        control.bg.Background = (Brush)Application.Current.Resources["Green"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["Green"];
                        break;
                    case OrderState.Running:
                        control.bg.Background = (Brush)Application.Current.Resources["Blue"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["Blue"];
                        break;
                    default:
                        control.bg.Background = (Brush)Application.Current.Resources["OnBackground"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["Background"];
                        break;
                }
                control.orderItems.ItemsSource = control.orderObject.OrderItems;

                control.LargeDiameterIndicator.Visibility = control.orderObject.OrderItems.First().MajorLength > 90 || control.orderObject.OrderItems.First().MajorDiameter > 20
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                control.editButton.Visibility = App.CurrentUser.UserRole is "Scheduling" or "admin"
                    ? Visibility.Visible
                    : Visibility.Collapsed;


                control.WarningBanner.Visibility = Visibility.Collapsed;
                int secondsBudgeted = 0;

                DateTime startDate = control.orderObject.StartDate;
                List<LatheManufactureOrderItem> orderedItems = control.orderObject.OrderItems.OrderBy(x => x.DateRequired).ToList();

                for (int i = 0; i < orderedItems.Count; i++)
                {
                    LatheManufactureOrderItem item = orderedItems[i];
                    if (item.RequiredQuantity == 0)
                    {
                        continue;
                    }

                    int secondsToRequirement = (item.RequiredQuantity-item.QuantityDelivered) * item.GetCycleTime();

                    int diff = (int)(item.DateRequired.Date - startDate.Date.AddSeconds(secondsBudgeted)).TotalSeconds;

                    if (diff < secondsToRequirement)
                    {
                        control.WarningBanner.Visibility = Visibility.Visible;
                        control.WarningText.Text = "A delivery target will be missed with high confidence";
                        break;
                    }
                    else if (diff * 0.9 < secondsToRequirement || diff <= 86400)
                    {
                        control.WarningBanner.Visibility = Visibility.Visible;
                        control.WarningText.Text = "A delivery target is at risk";
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
            SetDateWindow dateWindow = new(orderObject);
            dateWindow.ShowDialog();

            if (dateWindow.SaveExit)
            {
                orderObject.StartDate = dateWindow.SelectedDate.Date.AddHours(12);
                orderObject.AllocatedMachine = dateWindow.AllocatedMachine;
                DatabaseHelper.Update(orderObject);
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
    }
}
