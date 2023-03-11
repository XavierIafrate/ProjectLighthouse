using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Programs
{
    public class ProgramManagerViewModel : BaseViewModel
    {
        public List<NcProgram> Programs;

        private List<NcProgram> filteredPrograms;
        public List<NcProgram> FilteredPrograms
        {
            get { return filteredPrograms; }
            set
            {
                filteredPrograms = value;
                OnPropertyChanged();
            }
        }

        private string searchString = "";
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                Search();
                OnPropertyChanged();
            }
        }

        private NcProgram selectedProgram;
        public NcProgram SelectedProgram
        {
            get { return selectedProgram; }
            set
            {
                selectedProgram = value;
                OnPropertyChanged();
            }
        }


        public ProgramManagerViewModel()
        {
            LoadData();
            Search();
        }

        void LoadData()
        {
            Programs = DatabaseHelper.Read<NcProgram>();
            Programs.ForEach(x => x.ValidateAll());
        }

        void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                FilteredPrograms = new(Programs);
                if (FilteredPrograms.Count > 0)
                {
                    SelectedProgram = filteredPrograms[0];
                }
                else
                {
                    SelectedProgram = null;
                }
                return;
            }


            FilteredPrograms = Programs
                .Where(x => x.Name.ToLowerInvariant().Contains(SearchString.ToLowerInvariant()))
                .ToList();

            if (FilteredPrograms.Count > 0)
            {
                SelectedProgram = Programs[0];
            }
            else
            {
                SelectedProgram = null;
            }
        }
    }
}

