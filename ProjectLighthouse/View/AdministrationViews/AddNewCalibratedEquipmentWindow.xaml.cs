using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public partial class AddNewCalibratedEquipmentWindow : Window
    {
        public bool SaveExit;
        public CalibratedEquipment NewEquipment = new();

        public AddNewCalibratedEquipmentWindow()
        {
            InitializeComponent();
        }

        private void UKAS_required_Checked(object sender, RoutedEventArgs e)
        {
            NewEquipment.UKAS = true;
        }

        private void UKAS_required_Unchecked(object sender, RoutedEventArgs e)
        {
            NewEquipment.UKAS = false;
        }

        private void NumbersOnly(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewEquipment.SerialNumber = SerialNumberTextBox.Text.Trim();
        }

        private void InstrumentTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combobox)
            {
                NewEquipment.Type = combobox.SelectedItem.ToString();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseHelper.Insert(NewEquipment))
            {
                SaveExit = true;
                Close();
            }
        }

        private void MakeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewEquipment.Make = MakeTextBox.Text.Trim();
        }

        private void ModelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewEquipment.Model = ModelTextBox.Text.Trim();
        }

        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(IntervalTextBox.Text, out int i))
            {
                NewEquipment.CalibrationIntervalMonths = i;
            }
        }
    }
}
