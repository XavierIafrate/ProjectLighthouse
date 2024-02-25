using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class HolidayManager : UserControl
    {
        public ProductionSchedule Schedule
        {
            get { return (ProductionSchedule)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(ProductionSchedule), typeof(HolidayManager), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not HolidayManager control) return;
            if (control.Schedule is null) return;
            control.DefinedHolidaysListBox.ItemsSource = control.Schedule.Holidays;
        }

        public HolidayManager()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (HolidaySelectionCalendar.SelectedDate is not DateTime selection) return;

            if (Schedule.AddHoliday(selection))
            {
                DefinedHolidaysListBox.ItemsSource = null;
                DefinedHolidaysListBox.ItemsSource = Schedule.Holidays;
                AddButton.IsEnabled = false;
            }
        }

        private void HolidaySelectionCalendar_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (Schedule is null) return;

            if (HolidaySelectionCalendar.SelectedDate is not DateTime selection)
            {
                AddButton.IsEnabled = false;
                return;
            }

            AddButton.IsEnabled = !Schedule.Holidays.Contains(selection.ChangeTime(0, 0, 0, 0));
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Schedule is null) return;
            if (sender is not Button button) return;
            if (button.CommandParameter is not DateTime date) return;

            if(Schedule.RemoveHoliday(date))
            {
                DefinedHolidaysListBox.ItemsSource = null;
                DefinedHolidaysListBox.ItemsSource = Schedule.Holidays;
                //AddButton.IsEnabled = false;
                return;
            }
        }
    }
}
