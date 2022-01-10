using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class CalibrationViewModel : BaseViewModel
    {
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
                OnPropertyChanged("SelectedEquipment");
            }
        }


        public CalibrationViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            Equipment = DatabaseHelper.Read<CalibratedEquipment>();
            List<CalibrationCertificate> certs = DatabaseHelper.Read<CalibrationCertificate>();

            for (int i = 0; i < Equipment.Count; i++)
            {
                Equipment[i].Certificates = certs.Where(e => e.Instrument == Equipment[i].Name).ToList();
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
    }
}
