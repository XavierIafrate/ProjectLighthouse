using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ProjectLighthouse.View
{
    public partial class SetDateWindow : Window
    {
        private ScheduleItem Item { get; set; }
        public DateTime SelectedDate;
        public string AllocatedMachine;
        public bool SaveExit = false;
        private List<DateTime> significantDates { get; set; }

        public SetDateWindow(ScheduleItem item, HashSet<DateTime> setDates = null)
        {
            InitializeComponent();

            //significantDates = new List<DateTime>
            //{
            //    DateTime.Today.Date,
            //    DateTime.Today.Date.AddDays(1)
            //};

            Item = item;
            TitleText.Text = $"Editing: {Item.Name}";

            calendar.SelectedDate = Item.StartDate == DateTime.MinValue
                ? DateTime.Today.AddDays(1)
                : Item.StartDate;

            AllocatedMachine = Item.AllocatedMachine;
            machine.Text = AllocatedMachine;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDate = calendar.SelectedDate ?? DateTime.Today;
            SelectedDate.AddHours(12);
            AllocatedMachine = machine.Text;
            if (string.IsNullOrEmpty(AllocatedMachine))
            {
                SelectedDate = DateTime.MinValue;
                AllocatedMachine = null;
            }
            SaveExit = true;
            Close();
        }

        private void CalendarButton_Loaded(object sender, EventArgs e)
        {
            CalendarDayButton button = (CalendarDayButton)sender;
            DateTime date = (DateTime)button.DataContext;
            HighlightDay(button, date);
            button.DataContextChanged += new DependencyPropertyChangedEventHandler(calendarButton_DataContextChanged);
        }

        private void HighlightDay(CalendarDayButton button, DateTime date)
        {
            //button.Background = significantDates.Contains(date)
            //    ? Brushes.DodgerBlue
            //    : Brushes.White;
            //button.Foreground = significantDates.Contains(date)
            //    ? Brushes.White
            //    : Brushes.DarkGray;
        }

        private void calendarButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CalendarDayButton button = (CalendarDayButton)sender;
            DateTime date = (DateTime)button.DataContext;
            HighlightDay(button, date);
        }

    }
}
