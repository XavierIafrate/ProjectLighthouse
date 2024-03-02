using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
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

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayOrderStateProgress : UserControl
    {
        public OrderState OrderState
        {
            get { return (OrderState)GetValue(OrderStateProperty); }
            set { SetValue(OrderStateProperty, value); }
        }

        public static readonly DependencyProperty OrderStateProperty =
            DependencyProperty.Register("OrderState", typeof(OrderState), typeof(DisplayOrderStateProgress), new PropertyMetadata(OrderState.Problem, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOrderStateProgress control) return;
            OrderState state = control.OrderState;

            Grid.SetColumnSpan(control.track, 2);
            control.track.Background = (Brush)Application.Current.Resources["Red"];
            control.ProblemMark.Background = (Brush)Application.Current.Resources["Red"];

            control.ReadyCheckMark.Visibility = Visibility.Hidden;
            control.PreparedCheckMark.Visibility = Visibility.Hidden;
            control.RunningCheckMark.Visibility = Visibility.Hidden;
            control.CompleteCheckMark.Visibility = Visibility.Hidden;

            if (state == OrderState.Ready)
            {
                Grid.SetColumnSpan(control.track, 4);
                control.track.Background = (Brush)Application.Current.Resources["Green"];
                control.ProblemMark.Background = (Brush)Application.Current.Resources["Green"];

                control.ReadyCheckMark.Visibility = Visibility.Visible;
            }
            
            if (state == OrderState.Prepared)
            {
                Grid.SetColumnSpan(control.track, 6);
                control.track.Background = (Brush)Application.Current.Resources["Teal"];
                control.ProblemMark.Background = (Brush)Application.Current.Resources["Teal"];

                control.ReadyCheckMark.Visibility = Visibility.Visible;
                control.PreparedCheckMark.Visibility = Visibility.Visible;
            }

            if(state == OrderState.Running)
            {
                Grid.SetColumnSpan(control.track, 8);
                control.track.Background = (Brush)Application.Current.Resources["Blue"];
                control.ProblemMark.Background = (Brush)Application.Current.Resources["Blue"];

                control.ReadyCheckMark.Visibility = Visibility.Visible;
                control.PreparedCheckMark.Visibility = Visibility.Visible;
                control.RunningCheckMark.Visibility = Visibility.Visible;
            }
            
            if (state == OrderState.Running)
            {
                Grid.SetColumnSpan(control.track, 11);
                control.track.Background = (Brush)Application.Current.Resources["OnBackground"];
                control.ProblemMark.Background = (Brush)Application.Current.Resources["OnBackground"];

                control.ReadyCheckMark.Visibility = Visibility.Visible;
                control.PreparedCheckMark.Visibility = Visibility.Visible;
                control.RunningCheckMark.Visibility = Visibility.Visible;
                control.CompleteCheckMark.Visibility = Visibility.Visible;
            }


        }

        public DisplayOrderStateProgress()
        {
            InitializeComponent();
        }
    }
}
