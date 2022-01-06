using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.ComponentModel;
using System.Diagnostics;
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
            DisplayLMOScheduling control = d as DisplayLMOScheduling;


            if (control != null)
            {
                if (control.orderObject != null)
                {
                    control.DataContext = control.orderObject;

                    switch (control.orderObject.State)
                    {
                        case OrderState.Problem:
                            control.bg.Background = (Brush)Application.Current.Resources["materialError"];
                            control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialError"];
                            break;
                        case OrderState.Ready:
                            control.bg.Background = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                            control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                            break;
                        case OrderState.Prepared:
                            control.bg.Background = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                            control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                            break;
                        case OrderState.Running:
                            control.bg.Background = (Brush)Application.Current.Resources["materialPrimaryBlue"];
                            control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialPrimaryBlue"];
                            break;
                        default:
                            control.bg.Background = (Brush)Application.Current.Resources["materialError"];
                            control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialError"];
                            break;
                    }
                    control.orderItems.ItemsSource = control.orderObject.OrderItems;

                    control.LargeDiameterIndicator.Visibility = control.orderObject.OrderItems.First().MajorLength > 90 || control.orderObject.OrderItems.First().MajorDiameter > 20
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                }
                control.editButton.Visibility = App.CurrentUser.UserRole is "Scheduling" or "admin"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

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
                orderObject.StartDate = dateWindow.SelectedDate;
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
                Grid.SetRow(OrderItemsSubControl, 0);
            }
            else
            {
                Grid.SetColumn(OrderItemsSubControl, 0);
                Grid.SetRow(OrderItemsSubControl, 1);
            }
        }
    }
}
