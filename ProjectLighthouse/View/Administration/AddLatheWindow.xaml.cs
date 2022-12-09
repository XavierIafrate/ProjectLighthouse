using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace View.Administration
{
    public partial class AddLatheWindow : Window
    {
        private List<Lathe> ExistingLathes;
        public Lathe Lathe { get; set; }
        private bool editing;

        public bool SaveExit;
        public AddLatheWindow(List<Lathe> existingLathes, Lathe? lathe = null)
        {
            InitializeComponent();

            ExistingLathes = existingLathes;
            outOfServiceCheckBox.Visibility = Visibility.Collapsed;

            if (lathe != null)
            {
                editing = true;
                Lathe = lathe;
                PrefillUi();
            }
        }

        private void PrefillUi()
        {
            Title = "Edit Lathe";
            TitleText.Text = $"Editing '{Lathe.Id}'";

            idTextBox.Text = Lathe.Id;
            idTextBox.IsEnabled = false;

            fullNameTextBox.Text = Lathe.FullName;
            makeTextBox.Text = Lathe.Make;
            modelTextBox.Text = Lathe.Model;
            serialNumberTextBox.Text = Lathe.SerialNumber;
            partOffTextBox.Text = Lathe.PartOff.ToString("0.0");
            maxDiameterTextBox.Text = Lathe.MaxDiameter.ToString("0.0");
            maxLengthTextBox.Text = Lathe.MaxLength.ToString("0.0");
            ipAddressTextBox.Text = Lathe.IPAddress;
            controllerReferenceTextBox.Text = Lathe.ControllerReference;
            remarksTextBox.Text = Lathe.Remarks;

            outOfServiceCheckBox.Visibility = Visibility.Visible;
            outOfServiceCheckBox.IsChecked = Lathe.OutOfService;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void idTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void fullNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void makeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void modelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void serialNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void maxDiameterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void AllowNumbersAndPeriodOnly(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textBox.Text, e);
        }

        private void maxLengthTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void ipAddressTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void controllerReferenceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void remarksTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void partOffTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
