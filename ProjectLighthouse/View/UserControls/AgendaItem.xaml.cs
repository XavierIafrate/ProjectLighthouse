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
                control.DateText.Foreground = (Brush)App.Current.Resources["disabledGray"];
            }
            else
            {
                control.orderList.Visibility = Visibility.Visible;
                control.NoneIndicator.Visibility = Visibility.Collapsed;
            }

            if (control.Day.Orders.Count <= 1)
            {
                control.Warning.Background = (Brush)Application.Current.Resources["materialPrimaryGreen"];
            }
            else if (control.Day.Orders.Count == 2)
            {
                control.Warning.Background = (Brush)Application.Current.Resources["materialYellow"];
            }
            else
            {
                control.Warning.Background = (Brush)Application.Current.Resources["materialError"];
            }

            //if (control.Day.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday && control.Day.Orders.Count is 0)
            //{
            //    control.Visibility = Visibility.Collapsed;
            //}
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
