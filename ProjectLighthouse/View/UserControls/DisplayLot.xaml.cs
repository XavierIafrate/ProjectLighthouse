using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLot : UserControl
    {
        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(DisplayLot), new PropertyMetadata(null));


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
                control.EditButton.Visibility = control.Lot.Date.AddDays(3) > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (control.Lot.IsDelivered)
            {
                control.HelperText.Text = "Delivered";
                control.EditButton.Visibility = Visibility.Collapsed;

            }
            else if (control.Lot.IsAccepted)
            {
                control.HelperText.Text = control.Lot.AllowDelivery ? "Not Delivered" : "Delivery restricted";
                control.EditButton.Visibility = control.Lot.Date.AddDays(3) > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;

            }
            else
            {
                control.HelperText.Text = "Quarantined";
                control.EditButton.Visibility = Visibility.Visible;

            }

            if (App.CurrentUser.Role == UserRole.Administrator)
            {
                control.EditButton.Visibility = Visibility.Visible;
            }
        }

        public DisplayLot()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditCommand.Execute(Lot.ID);
        }

    }
}
