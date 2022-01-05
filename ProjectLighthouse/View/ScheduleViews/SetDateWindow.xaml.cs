using ProjectLighthouse.Model;
using System;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class SetDateWindow : Window
    {
        private ScheduleItem Item { get; set; }
        public DateTime SelectedDate;
        public string AllocatedMachine;
        public bool SaveExit = false;

        public SetDateWindow(ScheduleItem item)
        {
            InitializeComponent();
            Item = item;
            TitleText.Text = $"Editing: {Item.Name}";

            if (Item.StartDate == DateTime.MinValue)
            {
                calendar.SelectedDate = DateTime.Today.AddDays(1);
            }
            else
            {
                calendar.SelectedDate = Item.StartDate;
            }

            AllocatedMachine = Item.AllocatedMachine;
            machine.Text = AllocatedMachine;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDate = calendar.SelectedDate ?? DateTime.Today;
            SelectedDate.AddHours(8);
            AllocatedMachine = machine.Text;
            SaveExit = true;
            Close();
        }
    }
}
