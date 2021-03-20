using ProjectLighthouse.Model;
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
using System.Windows.Shapes;

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
