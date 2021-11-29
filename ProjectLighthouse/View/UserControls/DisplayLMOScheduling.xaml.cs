using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
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



        public CompleteOrder orderObject
        {
            get { return (CompleteOrder)GetValue(orderObjectProperty); }
            set { SetValue(orderObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for orderObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty orderObjectProperty =
            DependencyProperty.Register("orderObject", typeof(CompleteOrder), typeof(DisplayLMOScheduling), new PropertyMetadata(null, SetValues));


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLMOScheduling control = d as DisplayLMOScheduling;

            if (control != null)
            {
                control.DataContext = control.orderObject;
                switch (control.orderObject.Order.State)
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
                control.LargeDiameterIndicator.Visibility = control.orderObject.OrderItems.First().MajorLength > 90 || control.orderObject.OrderItems.First().MajorDiameter > 20
                ? Visibility.Visible
                : Visibility.Collapsed;
            }
            control.editButton.Visibility = App.CurrentUser.UserRole is "Scheduling" or "admin"
                ? Visibility.Visible
                : Visibility.Collapsed;


        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetDateWindow dateWindow = new(orderObject.Order);
            dateWindow.ShowDialog();

            if (dateWindow.SaveExit)
            {
                orderObject.Order.StartDate = dateWindow.SelectedDate;
                orderObject.Order.AllocatedMachine = dateWindow.AllocatedMachine;
                DatabaseHelper.Update(orderObject.Order);
                orderObject.NotifyEditMade();
            }
        }
    }
}
