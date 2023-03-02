using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Quality
{
    public partial class AddNewCalibratedEquipmentWindow : Window
    {
        public bool SaveExit;
        public CalibratedEquipment Equipment { get; set; }
        private List<CalibratedEquipment> EquipmentList;

        public AddNewCalibratedEquipmentWindow(List<CalibratedEquipment> existingEquipment, int? id = null)
        {
            InitializeComponent();
            EquipmentList = existingEquipment;
            SetComboBoxes();

            if (id != null)
            {
                CalibratedEquipment? target = existingEquipment.Find(x => x.Id == id);

                if (target is null)
                {
                    MessageBox.Show($"Failed to find equipment with ID '{id:0}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                Equipment = (CalibratedEquipment)target.Clone();
                this.Title = $"Editing: {Equipment.EquipmentId}";
                AddButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SaveButton.Visibility = Visibility.Collapsed;
                Equipment = new();
            }

            DataContext = this;
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            sqlite_sequence? sequence = DatabaseHelper.Read<sqlite_sequence>().ToList().Find(x => x.name == nameof(CalibratedEquipment));

            if (sequence is null)
            {
                MessageBox.Show("Failed to retrieve next ID in equipment sequence", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Equipment.EquipmentId = $"CE{sequence.seq + 1:0}";
            Equipment.EnteredSystem = System.DateTime.Now;
            Equipment.AddedBy = App.CurrentUser.UserName;

            try
            {
                DatabaseHelper.Insert(Equipment, throwErrs: true);
                SaveExit = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(IntervalTextBox.Text, out int i))
            {
                Equipment.CalibrationIntervalMonths = i;
            }
        }

        private void MakeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetModelComboBox();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseHelper.Update(Equipment))
            {
                SaveExit = true;
                Close();
            }
        }
    }
}
