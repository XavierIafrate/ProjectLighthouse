using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.View.Quality;
using ProjectLighthouse.ViewModel.Commands.Calibration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Quality
{
    public class CalibrationViewModel : BaseViewModel
    {
        #region Variables
        public List<CalibratedEquipment> Equipment { get; set; }
        public List<CalibrationCertificate> Certificates { get; set; }

        private CalibratedEquipment selectedEquipment;
        public CalibratedEquipment SelectedEquipment
        {
            get { return selectedEquipment; }
            set
            {
                selectedEquipment = value;
                LoadSelectedEquipment();
                OnPropertyChanged();
            }
        }

        public List<CalibratedEquipment> FilteredEquipment { get; set; }
        public List<CalibrationCertificate> FilteredCertificates { get; set; }


        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value;
                Search();
            }
        }

        private string selectedFilter = "Active";

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterEquipment();
                OnPropertyChanged();
            }
        }



        #region Commands
        public EditEquipmentCommand EditEquipmentCmd { get; set; }
        public AddCertificateCommand AddCertificateCmd { get; set; }
        public AddNewEquipmentCommand AddNewEquipmentCmd { get; set; }
        public GenerateReportCommand GenerateReportCmd { get; set; }
        public RecordVisualCheckCommand RecordVisualCheckCmd { get; set; }
        #endregion

        #region Visibilities
        private Visibility noCertsVis;

        public Visibility NoCertsVis
        {
            get { return noCertsVis; }
            set
            {
                noCertsVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility certsFound;

        public Visibility CertsFound
        {
            get { return certsFound; }
            set
            {
                certsFound = value;
                OnPropertyChanged();
            }
        }

        private Visibility cardVis;

        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility nothingVis;
        public Visibility NothingVis
        {
            get { return nothingVis; }
            set
            {
                nothingVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility regularCalVis;
        public Visibility RegularCalVis
        {
            get { return regularCalVis; }
            set
            {
                regularCalVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility calLapsedVis;
        public Visibility CalLapsedVis
        {
            get { return calLapsedVis; }
            set
            {
                calLapsedVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility indicationOnlyVis;
        public Visibility IndicationOnlyVis
        {
            get { return indicationOnlyVis; }
            set
            {
                indicationOnlyVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility outOfServiceVis;
        public Visibility OutOfServiceVis
        {
            get { return outOfServiceVis; }
            set
            {
                outOfServiceVis = value;
                OnPropertyChanged();
            }
        }

        public bool CanModify { get; set; }

        #endregion

        #endregion
        public CalibrationViewModel()
        {
            InitialiseVariables();
            Refresh();
        }

        private void InitialiseVariables()
        {
            EditEquipmentCmd = new(this);
            AddCertificateCmd = new(this);
            AddNewEquipmentCmd = new(this);
            GenerateReportCmd = new(this);
            RecordVisualCheckCmd = new(this);

            Certificates = new();
            Equipment = new();
            FilteredCertificates = new();
            FilteredEquipment = new();

            CanModify = App.CurrentUser.HasPermission(PermissionType.ModifyCalibration);
        }


        private void LoadData()
        {
            Equipment = DatabaseHelper.Read<CalibratedEquipment>(throwErrs: true);
            Certificates = DatabaseHelper.Read<CalibrationCertificate>();
        }

        private void LoadSelectedEquipment()
        {
            if (SelectedEquipment == null)
            {
                CardVis = Visibility.Hidden;
                NothingVis = Visibility.Visible;
                return;
            }

            NothingVis = Visibility.Hidden;
            CardVis = Visibility.Visible;

            RegularCalVis = Visibility.Collapsed;
            CalLapsedVis = Visibility.Collapsed;
            OutOfServiceVis = Visibility.Collapsed;
            IndicationOnlyVis = Visibility.Collapsed;

            if (SelectedEquipment.IsOutOfService)
            {
                OutOfServiceVis = Visibility.Visible;
            }
            else if (!SelectedEquipment.RequiresCalibration)
            {
                IndicationOnlyVis = Visibility.Visible;
            }
            else if (SelectedEquipment.CalibrationHasLapsed())
            {
                CalLapsedVis = Visibility.Visible;
            }
            else
            {
                RegularCalVis = Visibility.Visible;
            }

            FilteredCertificates = Certificates.Where(x => x.Instrument == SelectedEquipment.EquipmentId).OrderByDescending(x => x.DateIssued).ToList();

            OnPropertyChanged(nameof(FilteredCertificates));

            if (FilteredCertificates.Count > 0)
            {
                CertsFound = Visibility.Visible;
                NoCertsVis = Visibility.Collapsed;
            }
            else
            {
                CertsFound = Visibility.Collapsed;
                NoCertsVis = Visibility.Visible;
            }
        }

        private void FilterEquipment()
        {
            switch (selectedFilter)
            {
                case "Active":
                    FilteredEquipment = Equipment.Where(x => !x.IsOutOfService).ToList();
                    break;

                case "Out of Service":
                    FilteredEquipment = Equipment.Where(x => x.IsOutOfService).ToList();
                    break;

                case "Calibration Lapsed":
                    FilteredEquipment = Equipment.Where(x => x.CalibrationHasLapsed()).ToList();
                    break;

                case "Near Expiry":
                    FilteredEquipment = Equipment
                        .Where(x => x.CalibrationHasLapsed()
                                || System.DateTime.Now.AddMonths(1) > x.NextDue
                                && x.RequiresCalibration
                                && !x.IsOutOfService)
                        .ToList();
                    break;

                case "All Items":
                    FilteredEquipment = Equipment;
                    break;
            }

            OnPropertyChanged(nameof(FilteredEquipment));
            if (FilteredEquipment.Count > 0)
            {
                SelectedEquipment = FilteredEquipment[0];
            }
        }

        public void AddNewEquipment()
        {
            AddNewCalibratedEquipmentWindow window = new(Equipment) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();
            if (window.SaveExit)
            {
                int id = window.Equipment.Id;
                Refresh();
                SelectedEquipment = FilteredEquipment.Find(x => x.Id == id);
                if (SelectedEquipment is null && FilteredEquipment.Count > 0)
                {
                    SelectedEquipment = FilteredEquipment.First();
                }
            }
        }

        public void AddCertificate()
        {
            AddNewCalibrationCertificateWindow window = new(SelectedEquipment, Certificates) { Owner = App.MainViewModel.MainWindow }; ;
            window.ShowDialog();

            if (window.SaveExit)
            {
                int id = SelectedEquipment.Id;
                Refresh();
                SelectedEquipment = FilteredEquipment.Find(x => x.Id == id);
                if (SelectedEquipment is null && FilteredEquipment.Count > 0)
                {
                    SelectedEquipment = FilteredEquipment.First();
                }
            }
        }

        public void EditEquipment(int id)
        {
            AddNewCalibratedEquipmentWindow window = new(Equipment, id) { Owner = App.MainViewModel.MainWindow }; ;
            window.ShowDialog();
            if (window.SaveExit)
            {
                Refresh();
                SelectedEquipment = FilteredEquipment.Find(x => x.Id == id);
                if (SelectedEquipment is null && FilteredEquipment.Count > 0)
                {
                    SelectedEquipment = FilteredEquipment.First();
                }
            }
        }

        public void CreateReport()
        {
            List<CalibratedEquipment> requiresRecal = Equipment.Where(x => x.NextDue < System.DateTime.Now.AddMonths(2) && x.RequiresCalibration && !x.IsOutOfService).ToList();
            CSVHelper.WriteListToCSV(requiresRecal, "RequiresCal");
        }

        public void RecordVisualCheck()
        {
            MessageBox.Show("Record Visual Check");
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilterEquipment();
                return;
            }
            string searchFor = SearchTerm.ToUpperInvariant();

            List<string> matches = new();

            matches.AddRange(Equipment
                       .Where(e => e.EquipmentId.Contains(searchFor)
                    || e.SerialNumber.Contains(searchFor)
                    || e.Make.ToUpperInvariant().Contains(searchFor)
                    || e.Model.ToUpperInvariant().Contains(searchFor)
                    || e.Location.ToUpperInvariant().Contains(searchFor)
                    || e.Type.ToUpperInvariant().Contains(searchFor)
                    )
                .Select(e => e.EquipmentId));


            matches.AddRange(Certificates
                       .Where(c => c.CertificateNumber.ToUpperInvariant().Contains(searchFor)
                    || c.CalibrationHouse.ToUpperInvariant().Contains(searchFor)
                )
                .Select(c => c.Instrument));

            matches = matches.Distinct().ToList();

            FilteredEquipment = Equipment.Where(x => matches.Contains(x.EquipmentId)).ToList();
            OnPropertyChanged(nameof(FilteredEquipment));

            if (FilteredEquipment.Count > 0)
            {
                SelectedEquipment = FilteredEquipment[0];
            }
        }

        public void Refresh()
        {
            LoadData();
            FilterEquipment();
        }
    }
}
