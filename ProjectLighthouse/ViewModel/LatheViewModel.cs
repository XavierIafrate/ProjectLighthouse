

using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class LatheViewModel : BaseViewModel
    {
        private List<Lathe> lathes;
        public List<Lathe> Lathes
        {
            get { return lathes; }
            set 
            { 
                lathes = value;
                OnPropertyChanged();
            }
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


        public LatheViewModel()
        {
            GetData();
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
    }
}
