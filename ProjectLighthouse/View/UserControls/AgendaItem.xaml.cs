using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace ProjectLighthouse.View.UserControls
{
    public partial class AgendaItem : UserControl
    {
        public CalendarDay Day
        {
            get { return (CalendarDay)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static DependencyProperty DayProperty => dayProperty;

        private static readonly DependencyProperty dayProperty =
            DependencyProperty.Register("Day", typeof(CalendarDay), typeof(AgendaItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AgendaItem control)
            {
                return;
            }

            control.DataContext = control.Day;

            control.DateText.Text = control.Day.Date.ToString("dddd d") + GetDaySuffix(control.Day.Date.Day);

            if (control.Day.Orders.Count == 0)
            {
                control.orderList.Visibility = Visibility.Collapsed;
                control.NoneIndicator.Visibility = Visibility.Visible;
                control.DateText.Foreground = (Brush)App.Current.Resources["DisabledElement"];
                control.Counter.Foreground = (Brush)App.Current.Resources["DisabledElement"];
            }
            else
            {
                control.orderList.Visibility = Visibility.Visible;
                control.NoneIndicator.Visibility = Visibility.Collapsed;
                control.DateText.Foreground = (Brush)App.Current.Resources["OnSurface"];
                control.Counter.Foreground = (Brush)App.Current.Resources["OnSurface"];
            }

            if (control.Day.Orders.Count <= 1)
            {
                control.Warning.Background = (Brush)Application.Current.Resources["Green"];
            }
            else if (control.Day.Orders.Count == 2)
            {
                control.Warning.Background = (Brush)Application.Current.Resources["Orange"];
            }
            else
            {
                control.Warning.Background = (Brush)Application.Current.Resources["Red"];
            }

            if (control.Day.Date.DayOfWeek == DayOfWeek.Saturday || control.Day.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                if (control.Day.Orders.Count > 0)
                {
                    control.Warning.Background = (Brush)Application.Current.Resources["Red"];
                }
            }
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }

        public AgendaItem()
        {
            InitializeComponent();
        }
    }
}
