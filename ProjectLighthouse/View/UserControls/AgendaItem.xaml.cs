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

            if (control.Day.Orders.Count == 0)
            {
                control.orderList.Visibility = Visibility.Collapsed;
                control.DateText.Foreground = (Brush)App.Current.Resources["disabledGray"];
                control.DateText.FontSize = 18;
            }

            if (control.Day.Date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday && control.Day.Orders.Count is 0)
            {
                control.Visibility = Visibility.Collapsed;
            }
        }

        public AgendaItem()
        {
            InitializeComponent();
        }
    }
}
