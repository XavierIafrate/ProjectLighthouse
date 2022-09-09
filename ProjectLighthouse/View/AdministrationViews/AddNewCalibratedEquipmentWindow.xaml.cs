using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View
{
    public partial class AddNewCalibratedEquipmentWindow : Window
    {
        public bool SaveExit;
        public CalibratedEquipment NewEquipment;
        private List<CalibratedEquipment> EquipmentList;

        public AddNewCalibratedEquipmentWindow(List<CalibratedEquipment> existingEquipment)
        {
            InitializeComponent();
            EquipmentList = existingEquipment;
            SetComboBoxes();
            NewEquipment = new();
            DataContext = NewEquipment;
        }

        private void SetComboBoxes()
        {
            MakeComboBox.ItemsSource = EquipmentList.Select(x => x.Make).Distinct().OrderBy(x => x);
            LocationComboBox.ItemsSource = EquipmentList.Select(x => x.Location).Distinct().OrderBy(x => x);
            InstrumentTypeComboBox.ItemsSource = EquipmentList.Select(x => x.Type).Distinct().OrderBy(x => x);
        }

        private void SetModelComboBox()
        {
            if (MakeComboBox.SelectedValue != null)
            {
                ModelComboBox.ItemsSource = EquipmentList
                    .Where(x => x.Make == MakeComboBox.SelectedValue.ToString())
                    .Select(x => x.Model)
                    .Distinct()
                    .OrderBy(x => x);
            }
        }

        private void NumbersOnly(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void CleanValues()
        {
            NewEquipment.Make = NewEquipment.Make.Trim();
            NewEquipment.Model = NewEquipment.Model.Trim();
            NewEquipment.SerialNumber = NewEquipment.SerialNumber.ToUpperInvariant().Trim();
            NewEquipment.Type = NewEquipment.Type.Trim();
            NewEquipment.Location = NewEquipment.Location.ToUpperInvariant().Trim();
            if (!NewEquipment.RequiresCalibration)
            {
                UKAS_required.IsChecked = false;
            }
        }

        private bool Validate()
        {
            BrushConverter converter = new();
            Brush validBorder = (Brush)converter.ConvertFromString("#f0f0f0");
            bool noErrors = true;
            if (string.IsNullOrEmpty(NewEquipment.Make))
            {
                MakeTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                noErrors = false;
            }
            else
            {
                MakeTextBox.BorderBrush = validBorder;
            }

            if (string.IsNullOrEmpty(NewEquipment.Model))
            {
                ModelTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                noErrors = false;
            }
            else
            {
                ModelTextBox.BorderBrush = validBorder;
            }

            //if (string.IsNullOrEmpty(NewEquipment.SerialNumber))
            //{
            //    SerialNumberTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
            //    noErrors = false;
            //}
            //else
            //{
            //    SerialNumberTextBox.BorderBrush = validBorder;
            //}

            if (string.IsNullOrEmpty(NewEquipment.Type))
            {
                TypeTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                noErrors = false;
            }
            else
            {
                TypeTextBox.BorderBrush = validBorder;
            }

            if (string.IsNullOrEmpty(NewEquipment.Location))
            {
                LocationTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                noErrors = false;
            }
            else
            {
                LocationTextBox.BorderBrush = validBorder;
            }

            if (NewEquipment.UKAS && NewEquipment.CalibrationIntervalMonths == 0)
            {
                IntervalTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                noErrors = false;
            }
            else
            {
                IntervalTextBox.BorderBrush = validBorder;
            }

            return noErrors;
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CleanValues();

            if (!Validate())
            {
                return;
            }

            sqlite_sequence sequence = DatabaseHelper.Read<sqlite_sequence>().ToList().Find(x => x.name == nameof(CalibratedEquipment));
            NewEquipment.EquipmentId = $"CE{sequence.seq + 1:0}";
            NewEquipment.EnteredSystem = System.DateTime.Now;
            NewEquipment.AddedBy = App.CurrentUser.UserName;
            if (DatabaseHelper.Insert(NewEquipment))
            {
                SaveExit = true;
                Close();
            }
        }

        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(IntervalTextBox.Text, out int i))
            {
                NewEquipment.CalibrationIntervalMonths = i;
            }
        }

        private void MakeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetModelComboBox();
        }
    }
}
