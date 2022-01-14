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
    public class CalibrationViewModel : BaseViewModel
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
        #endregion

        #region Visibilities
        public Visibility EditControlsVis { get; set; } = Visibility.Visible;
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
            // CanEditCalibration level

            // New Window
        }

        public void AddCertificate()
        {
            // CanEditCalibration level

            // New Window
        }

        public void EditEquipment()
        {
            EditControlsVis = Visibility.Visible;
            OnPropertyChanged(nameof(EditControlsVis));

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
            /*
             * 
             *  Get .csv of items
             * 
             */
        }

        public void RecordVisualCheck()
        {

        }

    }
}
