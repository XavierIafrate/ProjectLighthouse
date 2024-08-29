using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class BreakdownEditor : UserControl
    {
        private List<BreakdownCode> breakdownCodes;

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(BreakdownEditor), new PropertyMetadata(false, SetCanEdit));

        private static void SetCanEdit(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BreakdownEditor control)
            {
                return;
            }

            control.AddNewBreakdownControls.Visibility = control.CanEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(BreakdownEditor), new PropertyMetadata(null));


        private MachineBreakdown newBreakdown = new();

        public MachineBreakdown NewBreakdown
        {
            get { return newBreakdown; }
            set { newBreakdown = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BreakdownEditor()
        {
            InitializeComponent();
            breakdownCodes = DatabaseHelper.Read<BreakdownCode>();
            this.BreakdownCodes_ComboBox.ItemsSource = this.breakdownCodes;
            if (this.breakdownCodes.Count > 0)
            {
                this.BreakdownCodes_ComboBox.SelectedIndex = 0;
            }
            DateTime now = DateTime.Now;
            NewBreakdown.BreakdownEnded = now.ChangeTime(now.Hour,0,0,0);
            NewBreakdown.BreakdownStarted = NewBreakdown.BreakdownEnded.AddHours(-1);
            if (breakdownCodes.Count > 0)
            {
                NewBreakdown.BreakdownCode = breakdownCodes.First().Id;
            }
        }

        private void AddBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (BreakdownCodes_ComboBox.SelectedValue is not BreakdownCode selectedBreakdownCode)
            {
                return;
            }

            if (!NewBreakdown.ValidateOverlap(Order.Breakdowns))
            {
                MessageBox.Show("New record overlaps time of other breakdown record", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.OrderName = Order.Name;
            NewBreakdown.CreatedAt = DateTime.Now;
            NewBreakdown.CreatedBy = App.CurrentUser.UserName;
            NewBreakdown.BreakdownCode = selectedBreakdownCode.Id;
            NewBreakdown.BreakdownMeta = selectedBreakdownCode;

            Order.Breakdowns = Order.Breakdowns.Append((MachineBreakdown)NewBreakdown.Clone()).ToList();

            NewBreakdown = new();
            DateTime now = DateTime.Now;
            NewBreakdown.BreakdownEnded = now.ChangeTime(now.Hour, 0, 0, 0);
            NewBreakdown.BreakdownStarted = NewBreakdown.BreakdownEnded.AddHours(-1);

            if (breakdownCodes.Count > 0)
            {
                NewBreakdown.BreakdownCode = breakdownCodes.First().Id;
            }
        }
    }
}
