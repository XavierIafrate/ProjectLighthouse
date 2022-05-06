using bpac;
using ProjectLighthouse.Model;
using System;
using System.Diagnostics;
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
                control.EditButton.Visibility = control.Lot.Date.AddDays(3) > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (control.Lot.IsDelivered)
            {
                control.HelperText.Text = "Delivered";
                control.EditButton.Visibility = Visibility.Collapsed;

            }
            else if (control.Lot.IsAccepted)
            {
                control.HelperText.Text = "Not Delivered";
                control.EditButton.Visibility = control.Lot.Date.AddDays(3) > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;

            }
            else
            {
                control.HelperText.Text = "Quarantined";
                control.EditButton.Visibility = Visibility.Visible;

            }

            if (App.CurrentUser.UserRole is "admin")
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
            Lot.NotifyRequestToEdit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"C:\Users\x.iafrate\Desktop\nameplate.lbx";
                Document doc = new();
                doc.Open(path);
                bool test = doc.SetPrinter(printerName: "Brother TD-4420DN", fitPage: true);
                doc.GetObject("productBarCode").Text = Lot.ProductName;
                doc.GetObject("quantity").Text = $"{Lot.Quantity:#,##0} PCS";
                doc.GetObject("orderId").Text = Lot.Order;
                doc.GetObject("lotId").Text = $"#{Lot.ID}";
                doc.GetObject("lotDate").Text = $"{Lot.Date:dd/MM/yyyy HH:mm}";
                doc.GetObject("printedTime").Text = $"{DateTime.Now:dd/MM/yyyy HH:mm}";
                doc.GetObject("printedBy").Text = $"by {App.CurrentUser.GetFullName().ToUpperInvariant()}";
                doc.StartPrint(docName:"", PrintOptionConstants.bpoHighResolution);
                doc.PrintOut(copyCount:1, PrintOptionConstants.bpoHighResolution);
                doc.EndPrint();
                doc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }
}
