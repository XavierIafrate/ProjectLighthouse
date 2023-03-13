using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

        public List<ProductGroup> Archetypes { get; set; }

        private List<ProductGroup>? usedFor;
        public List<ProductGroup>? UsedFor
        {
            get { return usedFor; }
            set 
            { 
                if (value is List<ProductGroup> list)
                {
                    if (list.Count == 0) value = null;
                }
                usedFor = value; 
                OnPropertyChanged();
            }
        }


        List<Note> Notes;

        private List<Note> filteredNotes;
        public List<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set 
            { 
                filteredNotes = value;
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
                LoadProgram();
                OnPropertyChanged();
            }
        }

        private string newMessage;
        public string NewMessage
        {
            get { return newMessage; }
            set 
            { 
                newMessage = value; 
                OnPropertyChanged();
            }
        }

        public SendMessageCommand SendMessageCmd { get; set; }

        public ProgramManagerViewModel()
        {
            LoadData();
            Search();
        }

        void LoadData()
        {
            SendMessageCmd = new(this);

            Programs = DatabaseHelper.Read<NcProgram>()
                .OrderBy(x => x.Name)
                .ToList();
            Programs.ForEach(x => x.ValidateAll());

            Notes = DatabaseHelper.Read<Note>()
                .Where(x => x.DocumentReference.StartsWith("p"))
                .ToList();

            Archetypes = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .ToList();
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

            string tagSearch = SearchString.Replace(" ", "").ToLowerInvariant();
            FilteredPrograms = Programs
                .Where(x => 
                x.Name.ToLowerInvariant().Contains(SearchString.ToLowerInvariant()) || 
                x.SearchableTags.Contains(tagSearch))
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

        public void SendMessage()
        {
            if (SelectedProgram is null) return;

            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"p{SelectedProgram.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            try
            {
                DatabaseHelper.Insert(newNote, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Notes.Add(newNote);
            LoadProgram();

            NewMessage = "";
        }

        private void LoadProgram()
        {
            if (SelectedProgram == null)
            {
                FilteredNotes = null;
                return;
            }

            FilteredNotes = Notes
                .Where(x => x.DocumentReference == $"p{SelectedProgram.Id}")
                .ToList();

            UsedFor = Archetypes
                .Where(x => x.ProgramId == SelectedProgram.Id)
                .ToList();
        }
    }
}

