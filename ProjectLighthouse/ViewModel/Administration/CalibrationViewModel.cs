using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Commands.Calibration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class CalibrationViewModel : BaseViewModel, IRefreshableViewModel
    {
        #region Variables
        private List<CalibratedEquipment> equipment;
        public List<CalibratedEquipment> Equipment
        {
            get { return equipment; }
            set 
            {
                equipment = value;
                OnPropertyChanged("Equipment");
            }
        }

        private CalibratedEquipment selectedEquipment;
        public CalibratedEquipment SelectedEquipment
        {
            get { return selectedEquipment; }
            set 
            { 
                selectedEquipment = value;
                Certificates = selectedEquipment.Certificates;
                OnPropertyChanged("Certificates");
                OnPropertyChanged("SelectedEquipment");
            }
        }

        public List<CalibrationCertificate> Certificates { get; set; }

        #region Commands
        public EditEquipmentCommand EditEquipmentCmd { get; set; }
        public AddCertificateCommand AddCertificateCmd { get; set; }
        public AddNewEquipmentCommand AddNewEquipmentCmd { get; set; }
        public GenerateReportCommand GenerateReportCmd { get; set; }
        public RecordVisualCheckCommand RecordVisualCheckCmd { get; set; }
        #endregion

        #region Visibilities
        public Visibility EditControlsVis { get; set; } = Visibility.Collapsed;
        public bool StopRefresh { get; set; }
        #endregion

        #endregion
        public CalibrationViewModel()
        {
            LoadData();
            LoadCommands();
            if (Equipment.Count > 0)
            {
                SelectedEquipment = Equipment.First();
            }
        }

        private void LoadCommands()
        {
            EditEquipmentCmd = new(this);
            AddCertificateCmd = new(this);
            AddNewEquipmentCmd = new(this);
            GenerateReportCmd = new(this);
            RecordVisualCheckCmd = new(this);
        }


        private void LoadData()
        {
            Certificates = new();

            Equipment = DatabaseHelper.Read<CalibratedEquipment>();
            List<CalibrationCertificate> certs = DatabaseHelper.Read<CalibrationCertificate>();

            for (int i = 0; i < Equipment.Count; i++)
            {
                Equipment[i].Certificates = certs.Where(e => e.Instrument == Equipment[i].Name).ToList();
                Equipment[i].RequestToEdit += EditEquipment;
            }
        }

        public void AddNewEquipment()
        {
            MessageBox.Show("Add New Equipment");
            // CanEditCalibration level

            // New Window
        }

        public void AddCertificate()
        {
            MessageBox.Show("Add New Certificate");
            // CanEditCalibration level

            // New Window
        }

        public void EditEquipment()
        {
            EditControlsVis = Visibility.Visible;
            OnPropertyChanged(nameof(EditControlsVis));
            MessageBox.Show("Edit Equipment");

            /*
             * 
             *  Admin and CanEditCalibration different priviledges
             *  
             *  admin only: edit SN, Make, Model, CalInterval, Type, RequiresCalibration, UKAS
             *  
             *  remaining: Location, LastVisualCheck, LastCalibrated, CalibrationHouse, MarkFailed, Notes
             * 
             */
        }

        public void CreateReport()
        {
            MessageBox.Show("Generate Report");

            /*
             * 
             *  Get .csv of items
             * 
             */
        }

        public void RecordVisualCheck()
        {
            MessageBox.Show("Record Visual Check");
        }

        public void Refresh()
        {
            LoadData();
        }
    }
}
