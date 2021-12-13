using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLot : UserControl
    {
        public Lot Lot
        {
            get { return (Lot)GetValue(LotProperty); }
            set { SetValue(LotProperty, value); }
        }

        private static readonly DependencyProperty LotProperty =
            DependencyProperty.Register("Lot", typeof(Lot), typeof(DisplayLot), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayLot control)
            {
                return;
            }

            control.DataContext = control.Lot;

            if (control.Lot.IsReject)
            {
                control.HelperText.Text = "Rejected";
            }
            else if (control.Lot.IsDelivered)
            {
                control.HelperText.Text = "Delivered";
            }
            else if (control.Lot.IsAccepted)
            {
                control.HelperText.Text = "Not Delivered";
            }
            else
            {
                control.HelperText.Text = "Quarantined";
            }

            if (control.Lot.IsDelivered || control.Lot.Date.AddDays(14) < DateTime.Now)
            {
                control.EditButton.Visibility = Visibility.Collapsed;
            }
        }

        public DisplayLot()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Lot.NotifyRequestToEdit();
        }
    }
}
