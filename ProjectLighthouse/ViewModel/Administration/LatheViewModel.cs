using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using View.Administration;
using ViewModel.Commands.Administration;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class LatheViewModel : BaseViewModel
    {
        public List<Lathe> Lathes { get; set; }
        private List<Lathe> filteredLathes;

        public List<Lathe> FilteredLathes
        {
            get { return filteredLathes; }
            set { filteredLathes = value; OnPropertyChanged(); }
        }

        private Lathe selectedLathe;

        public Lathe SelectedLathe
        {
            get { return selectedLathe; }
            set { selectedLathe = value; OnPropertyChanged(); }
        }

        private List<MaintenanceEvent> maintenanceEvents;
        public List<MaintenanceEvent> MaintenanceEvents
        {
            get { return maintenanceEvents; }
            set
            {
                maintenanceEvents = value;
                OnPropertyChanged();
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value.ToUpper();
                OnPropertyChanged();
                FilterData();
            }
        }

        public AddMaintenanceEventCommand AddMaintenanceEventCmd { get; set; }


        public LatheViewModel()
        {
            AddMaintenanceEventCmd = new(this);

            GetData();
            FilterData();

            if (FilteredLathes.Count > 0)
            {
                SelectedLathe = Lathes[0];
            }
        }

        private void GetData()
        {
            Lathes = DatabaseHelper.Read<Lathe>().ToList();
            MaintenanceEvents = DatabaseHelper.Read<MaintenanceEvent>().ToList();

            foreach (Lathe lathe in Lathes)
            {
                lathe.Maintenance = MaintenanceEvents
                    .Where(x => x.Lathe == lathe.Id)
                    .ToList();
            }
        }

        private void FilterData()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredLathes = new(Lathes);
                return;
            }

            FilteredLathes = Lathes.Where(x =>
               x.IPAddress.Contains(SearchTerm)
            || x.FullName.ToUpperInvariant().Contains(SearchTerm)
            || x.Id.ToUpperInvariant().Contains(SearchTerm)
            || x.Make.ToUpperInvariant().Contains(SearchTerm)
            || x.Model.ToUpperInvariant().Contains(SearchTerm)
            ).ToList();
        }

        public void AddMaintenanceEvent()
        {
            CreateMaintenanceEventWindow window = new(SelectedLathe);
            window.Show();
        }
    }
}
