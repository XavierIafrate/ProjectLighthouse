using Microsoft.Win32;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ProjectLighthouse.View.Quality
{
    public partial class AddNewCalibrationCertificateWindow : Window
    {
        CalibratedEquipment Equipment;
        public bool SaveExit;
        CalibrationCertificate NewCertificate { get; set; }
        List<CalibrationCertificate> CertificateList { get; set; }
        string targetPdf = "";
        string destinationPdf = "";

        public AddNewCalibrationCertificateWindow(CalibratedEquipment equipment, List<CalibrationCertificate> certificates)
        {
            InitializeComponent();
            Equipment = equipment;
            NewCertificate = new() { Instrument = Equipment.EquipmentId, DateIssued = System.DateTime.Today.AddDays(-1) };
            CertificateList = certificates;
            SetComboBoxes();
            DataContext = NewCertificate;
        }

        private void SetComboBoxes()
        {
            CalHouseComboBox.ItemsSource = CertificateList.Select(x => x.CalibrationHouse).Distinct();
            datePicker.DisplayDateEnd = System.DateTime.Today;
            datePicker.DisplayDateStart = System.DateTime.Today.AddMonths(-Equipment.CalibrationIntervalMonths - 1);
        }

        private void CleanValues()
        {
            NewCertificate.CertificateNumber = NewCertificate.CertificateNumber.Trim();
            NewCertificate.CalibrationHouse = NewCertificate.CalibrationHouse.Trim();
        }

        private bool Validate()
        {
            BrushConverter cnv = new();
            Brush okBrush = (Brush)cnv.ConvertFromString("#f0f0f0");
            bool NoErrors = true;
            if (Equipment.UKAS && Equipment.RequiresCalibration)
            {
                if (!NewCertificate.UKAS)
                {
                    MessageBox.Show($"{Equipment.EquipmentId} requires UKAS Accredited calibration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!NewCertificate.IsPass)
            {
                if (MessageBox.Show("Are you sure the equipment has failed calibration?" + Environment.NewLine + "It will be removed from service.", "Confirm Result", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    NoErrors = false;
                }
            }

            if (string.IsNullOrEmpty(NewCertificate.CertificateNumber))
            {
                NoErrors = false;
                CertNumTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                CertNumTextBox.BorderBrush = okBrush;
            }

            if (string.IsNullOrEmpty(NewCertificate.CalibrationHouse))
            {
                NoErrors = false;
                CalHouseTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                CalHouseTextBox.BorderBrush = okBrush;
            }

            if (NewCertificate.DateIssued > DateTime.Today
                || NewCertificate.DateIssued < Equipment.LastCalibrated) //NewCertificate.DateIssued < DateTime.Today.AddMonths(-Equipment.CalibrationIntervalMonths -1 ) || 
            {
                NoErrors = false;
                datePicker.BorderBrush = (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                datePicker.BorderBrush = okBrush;
            }

            if (string.IsNullOrEmpty(targetPdf))
            {
                NoErrors = false;
                fileDisplay.Foreground = (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                fileDisplay.Foreground = (Brush)Application.Current.Resources["OnBackground"];
            }

            return NoErrors;
        }
        private void ChoosePDFButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() ?? false)
            {
                targetPdf = openFileDialog.FileName;
                fileDisplay.Text = Path.GetFileName(openFileDialog.FileName);
            }
        }

        private bool MovePdf()
        {
            NewCertificate.Url = @"Calibration\" + @$"{MakeValidFileName($"{Equipment.EquipmentId}_{NewCertificate.CertificateNumber}")}.pdf";
            destinationPdf = Path.Combine(App.ROOT_PATH, NewCertificate.Url);

            try
            {
                File.Copy(targetPdf, destinationPdf);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return false;
            }
        }

        private static string MakeValidFileName(string name)
        {
            name = name.Trim().ToUpperInvariant();
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CleanValues();

            if (!Validate())
            {
                return;
            }

            if (!MovePdf())
            {
                return;
            }

            NewCertificate.AddedBy = App.CurrentUser.UserName;
            NewCertificate.AddedAt = DateTime.Now;

            Equipment.LastCalibrated = NewCertificate.DateIssued;
            Equipment.IsOutOfService = !NewCertificate.IsPass;
            try
            {
                DatabaseHelper.Insert(NewCertificate);
                Equipment.IsOutForCal = false;
                DatabaseHelper.Update(Equipment);
                SaveExit = true;
                Close();
            }
            catch
            {
                return;
            }

        }
    }
}
