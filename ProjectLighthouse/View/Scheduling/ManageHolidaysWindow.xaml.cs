using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class ManageHolidaysWindow : Window
    {
        public bool SaveExit;
        public List<DateTime> Holidays;
        public ManageHolidaysWindow(List<DateTime> holidays)
        {
            this.Holidays = holidays;
            InitializeComponent();

            holidaysListView.ItemsSource = this.Holidays;
        }

        private void calendar_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (calendar.SelectedDate is null)
            {
                addButton.IsEnabled = false;
                return;
            }

            addButton.IsEnabled = !Holidays.Contains(calendar.SelectedDate.Value.Date);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (calendar.SelectedDate is null) return;
            Holidays.Add(calendar.SelectedDate.Value.Date);
            Holidays = Holidays.OrderBy(x => x).ToList();

            holidaysListView.Items.Refresh();
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not DateTime date) return;

            Holidays.Remove(date);

            holidaysListView.Items.Refresh();
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            SaveExit = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
