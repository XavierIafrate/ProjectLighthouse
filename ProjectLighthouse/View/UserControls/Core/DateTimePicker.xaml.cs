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

namespace ProjectLighthouse.View.UserControls
{
    public partial class DateTimePicker : UserControl
    {
        public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("DateTime", typeof(DateTime), typeof(DateTimePicker), new PropertyMetadata(DateTime.Now, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DateTimePicker control) return;

            control.datePicker.SelectedDate = control.DateTime;
            control.timeText.Text = control.DateTime.ToString("HH:mm");
        }

        public DateTimePicker()
        {
            InitializeComponent();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!this.IsLoaded) return;
            this.DateTime = this.datePicker.SelectedDate ?? DateTime.MinValue;
        }

        private void largeDecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime < DateTime.MinValue.AddHours(1)) return;
            this.DateTime = this.DateTime.AddHours(-1);
            timeText.Text = DateTime.ToString("HH:mm");
        }

        private void smallDecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime < DateTime.MinValue.AddMinutes(1)) return;
            this.DateTime = this.DateTime.AddMinutes(-1);
            timeText.Text = DateTime.ToString("HH:mm");
        }

        private void largeIncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime > DateTime.MaxValue.AddHours(-1)) return;
            this.DateTime = this.DateTime.AddHours(1);
            timeText.Text = DateTime.ToString("HH:mm");
        }

        private void smallIncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime > DateTime.MaxValue.AddMinutes(-1)) return;
            this.DateTime = this.DateTime.AddMinutes(1);
            timeText.Text = DateTime.ToString("HH:mm");
        }
    }
}
