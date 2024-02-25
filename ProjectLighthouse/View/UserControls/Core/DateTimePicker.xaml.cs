using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public event EventHandler DateChanged;

        public DateTimePicker()
        {
            InitializeComponent();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;

            DateTime newDateTime;
            if (this.datePicker.SelectedDate == null)
            {
                newDateTime = DateTime.MinValue;
            }
            else
            {
                newDateTime = (DateTime)this.datePicker.SelectedDate;
                newDateTime = newDateTime.ChangeTime(this.DateTime.Hour, this.DateTime.Minute, 0, 0);
            }
            
            this.DateTime = newDateTime;

            DateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void largeDecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime < DateTime.MinValue.AddHours(1)) return;
            this.DateTime = this.DateTime.AddHours(-1);
            timeText.Text = DateTime.ToString("HH:mm");
        
            DateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void smallDecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime < DateTime.MinValue.AddMinutes(1)) return;
            this.DateTime = this.DateTime.AddMinutes(-1);
            timeText.Text = DateTime.ToString("HH:mm");
         
            DateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void largeIncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime > DateTime.MaxValue.AddHours(-1)) return;
            this.DateTime = this.DateTime.AddHours(1);
            timeText.Text = DateTime.ToString("HH:mm");

            DateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void smallIncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DateTime > DateTime.MaxValue.AddMinutes(-1)) return;
            this.DateTime = this.DateTime.AddMinutes(1);
            timeText.Text = DateTime.ToString("HH:mm");

            DateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
