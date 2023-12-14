using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class SetDateWindow : Window
    {
        private ScheduleItem Item { get; set; }
        public DateTime SelectedDate;
        public string AllocatedMachine;
        public bool SaveExit = false;

        List<Lathe> AvailableMachines = new();

        public SetDateWindow(ScheduleItem item)
        {
            InitializeComponent();

            AvailableMachines = DatabaseHelper.Read<Lathe>().Where(l => !l.OutOfService).Append(new() { FullName = "Unallocated" }).ToList();

            machine.ItemsSource = AvailableMachines;

            if (string.IsNullOrEmpty(item.AllocatedMachine))
            {
                machine.SelectedValue = AvailableMachines.Last();
            }
            else
            {
                machine.SelectedValue = AvailableMachines.Find(x => x.Id == item.AllocatedMachine);
            }

            Item = item;
            OrderText.Text = $"Item: '{Item.Name}'";
            TimeText.Text = $"{Item.StartDate:HH}:00";

            calendar.SelectedDate = Item.StartDate == DateTime.MinValue
                ? DateTime.Today.AddDays(1)
                : Item.StartDate;

            SelectedDate = (DateTime)calendar.SelectedDate;

            AllocatedMachine = Item.AllocatedMachine;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime timePart = SelectedDate;
            SelectedDate = calendar.SelectedDate ?? DateTime.Today;
            SelectedDate = SelectedDate.ChangeTime(timePart.Hour, timePart.Minute, timePart.Second, 0);

            if (machine.SelectedValue is Lathe l)
            {
                AllocatedMachine = l.Id;
            }
            else
            {
                AllocatedMachine = "";
            }

            if (string.IsNullOrEmpty(AllocatedMachine))
            {
                SelectedDate = DateTime.MinValue;
                AllocatedMachine = null;
            }

            SaveExit = true;
            Close();
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            int hourCurrent = SelectedDate.Hour;
            hourCurrent++;
            hourCurrent = hourCurrent % 24;
            SelectedDate = SelectedDate.ChangeTime(hourCurrent, 0, 0, 0);
            TimeText.Text = $"{SelectedDate:HH}:00";
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            int hourCurrent = SelectedDate.Hour;
            hourCurrent--;
            if (hourCurrent < 0)
            {
                hourCurrent = 23;
            }

            SelectedDate = SelectedDate.ChangeTime(hourCurrent, 0, 0, 0);

            TimeText.Text = $"{SelectedDate:HH}:00";
        }
    }
}
