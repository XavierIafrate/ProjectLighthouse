using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Programs;
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
        public List<Product> Products { get; set; }

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
        public EditProgramCommand EditProgramCmd { get; set; }

        public ProgramManagerViewModel()
        {
            LoadData();
            Search();
        }

        void LoadData()
        {
            SendMessageCmd = new(this);
            EditProgramCmd = new(this);

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
            Products = DatabaseHelper.Read<Product>()
                .OrderBy(x => x.Name)
                .ToList();
        }

        public void EditProgram()
        {
            if (SelectedProgram is null) return;

            AddProgramWindow window = new(Products, Archetypes, new(), new(), SelectedProgram) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int selectedProgramId = SelectedProgram.Id;

            LoadData();
            Search();

            SelectedProgram = Programs.Find(x => x.Id == selectedProgramId);
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

            List<int> groups = Archetypes.Where(x => x.Name.ToLowerInvariant().Contains(tagSearch)).Select(x => x.Id).ToList();
            FilteredPrograms.AddRange(Programs.Where(x => groups.Any(y => x.GroupStringIds.Contains(y.ToString("0")))));

            FilteredPrograms = FilteredPrograms.Distinct().ToList();

            if (FilteredPrograms.Count > 0)
            {
                SelectedProgram = FilteredPrograms[0];
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

            if (SelectedProgram.Groups is null) return;

            List<string> groupStringIds = SelectedProgram.Groups.Split(";").ToList();
            List<int> groupIds = new();

            for (int i = 0; i < groupStringIds.Count; i++)
            {
                if (!int.TryParse(groupStringIds[i], out int group))
                {
                    MessageBox.Show($"Failed to convert {groupStringIds[i]} to integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                groupIds.Add(group);
            }

            List<ProductGroup> targetedGroups = new();

            for (int i = 0; i < groupIds.Count; i++)
            {
                int id = groupIds[i];
                ProductGroup? g = Archetypes.Find(x => x.Id == id);

                if (g is null)
                {
                    MessageBox.Show($"Failed to find group with ID {id}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                targetedGroups.Add(g);
            }

            UsedFor = targetedGroups.OrderBy(x => x.Name).ToList();
        }
    }
}

