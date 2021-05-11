using ProjectLighthouse.Model;
using System;
using System.Windows;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for SetDateWindow.xaml
    /// </summary>
    public partial class SetDateWindow : Window
    {
        private LatheManufactureOrder Order { get; set; }
        public DateTime SelectedDate;
        public string AllocatedMachine;

        public SetDateWindow(LatheManufactureOrder order)
        {
            InitializeComponent();
            Order = order;

            if (Order.StartDate == DateTime.MinValue)
            {
                calendar.SelectedDate = DateTime.Today.AddDays(1);
            }
            else
            {
                calendar.SelectedDate = order.StartDate;
            }

            AllocatedMachine = order.AllocatedMachine;
            machine.Text = AllocatedMachine;
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDate = calendar.SelectedDate ?? DateTime.Today;
            SelectedDate.AddHours(8);
            AllocatedMachine = machine.Text;
            this.Close();
        }
    }
}
