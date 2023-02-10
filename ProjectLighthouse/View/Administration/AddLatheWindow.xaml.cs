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
        public Lathe? originalLathe;
        private bool editing;

        public bool SaveExit;
        public AddLatheWindow(List<Lathe> existingLathes, Lathe? lathe = null)
        {
            InitializeComponent();

            ExistingLathes = existingLathes;

            if (lathe is not null)
            {
                editing = true;
                originalLathe = lathe;
                Lathe = (Lathe)originalLathe.Clone();
            }
            else
            {
                Lathe = new();
            }
            SetUiElements();

            DataContext = this;
        }

        private void SetUiElements()
        {
            bool editing = originalLathe is not null;
            Title = editing ? "Edit Lathe" : "Add Lathe";
            TitleText.Text = editing ? $"Editing '{Lathe.Id}'" : "New Lathe";
            SubmitButton.Content = editing ? "Update" : "Create";
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
