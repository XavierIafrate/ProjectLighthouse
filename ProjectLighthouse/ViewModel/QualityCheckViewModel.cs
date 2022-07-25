using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel
{
    public class QualityCheckViewModel : BaseViewModel
    {
        public List<QualityCheck> Checks { get; set; }
        public List<Note> Notes { get; set; }

        private List<Note> filteredNotes;

        public List<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set { filteredNotes = value; OnPropertyChanged(); }
        }

        private List<QualityCheck> filteredChecks;

        public List<QualityCheck> FilteredChecks
        {
            get { return filteredChecks; }
            set { filteredChecks = value; OnPropertyChanged(); }
        }

        private QualityCheck selectedCheck;

        public QualityCheck SelectedCheck
        {
            get { return selectedCheck; }
            set 
            { 
                selectedCheck = value; 
                LoadCheck(); 
                OnPropertyChanged(); 
                
            }
        }

        public bool NoCheckFound { get; set; }

        public QualityCheckViewModel()
        {
            Checks = DatabaseHelper.Read<QualityCheck>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();
        }


        void FilterNotes(string searchTerm = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                FilteredChecks = Checks.Where(x => x.State < QualityCheck.Status.Complete || x.UpdatedAt.AddDays(7) > DateTime.Now).ToList();
            }
            else
            {
                FilteredChecks = Checks.Where(x => x.Product.ToUpper().Contains(searchTerm)).ToList();
            }

            if (FilteredChecks.Count > 0)
            {
                SelectedCheck = FilteredChecks.First();
            }
            else
            {
                SelectedCheck = null;
            }
        }


        void LoadCheck()
        {
            NoCheckFound = SelectedCheck == null;
            OnPropertyChanged(nameof(NoCheckFound));

            if (SelectedCheck == null)
            {
                return;
            }

            FilteredNotes = Notes.Where(x => x.DocumentReference == "q" + SelectedCheck.Id).ToList();
            OnPropertyChanged(nameof(FilteredNotes));
        }
    }
}
