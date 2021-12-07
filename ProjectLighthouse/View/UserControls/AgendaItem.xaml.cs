using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectLighthouse.Model;
namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for AgendaItem.xaml
    /// </summary>
    public partial class AgendaItem : UserControl
    {
        public CalendarDay Day
        {
            get { return (CalendarDay)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static DependencyProperty DayProperty => dayProperty;

        // Using a DependencyProperty as the backing store for Day.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty dayProperty =
            DependencyProperty.Register("Day", typeof(CalendarDay), typeof(AgendaItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AgendaItem control)
            {
                return;
            }

            control.DataContext = control.Day;

            //if (control.Day.Date.DayOfWeek == DayOfWeek.Monday)
            //{
            //    control.ControlGrid.Margin = new(10,20,10,10);
            //}

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
