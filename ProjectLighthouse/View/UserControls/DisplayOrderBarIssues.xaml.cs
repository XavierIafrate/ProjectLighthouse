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
using ProjectLighthouse.Model.Orders;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayOrderBarIssues : UserControl
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Order.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(DisplayOrderBarIssues), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOrderBarIssues control) return;

            control.OrderNameTextBox.Text = control.Order.Name;
            control.IssuesListBox.ItemsSource = control.Order.BarIssues;
            control.NumBarsTextBox.Text = $"{control.Order.NumberOfBars:0} Bars Required";

            control.IssueRequestedBadge.Visibility = control.Order.RequiresBar() 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            if (control.Order.StartDate.Year == DateTime.MinValue.Year)
            {
                control.StartDateTextBox.Text = "Awaiting Scheduling";
            }
            else
            {
                control.StartDateTextBox.Text = $"Set Date: {control.Order.StartDate:dd/MM/yyyy}";
            }

            if (control.Order.NumberOfBarsIssued == 0)
            {
                control.StatusText.Text = "No Bars Issued";
                control.StatusText.Foreground = (Brush)App.Current.Resources["Red"];
                control.IssuesListBox.Visibility = Visibility.Collapsed;
            }
            else if (control.Order.NumberOfBarsIssued < control.Order.NumberOfBars)
            {
                control.StatusText.Text = "Partial Bar Issue";
                control.StatusText.Foreground = (Brush)App.Current.Resources["Orange"];
                control.IssuesListBox.Visibility = Visibility.Visible;
            }
            else
            {
                control.StatusText.Text = "Balance Issued";
                control.StatusText.Foreground = (Brush)App.Current.Resources["Green"];
                control.IssuesListBox.Visibility = Visibility.Visible;
            }
        }

        public DisplayOrderBarIssues()
        {
            InitializeComponent();
        }
    }
}
