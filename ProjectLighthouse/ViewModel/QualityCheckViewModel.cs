using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private string searchTerm;

        public string SearchTerm
        {
            get { return searchTerm; }
            set { searchTerm = value; FilterChecks(value); OnPropertyChanged(); }
        }

        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged(); }
        }

        public bool NoCheckFound { get; set; }

        public SendMessageCommand SendMessageCommand { get; set; }

        public QualityCheckViewModel()
        {
            Checks = DatabaseHelper.Read<QualityCheck>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();
            SendMessageCommand = new(this);
            FilterChecks();
        }


        void FilterChecks(string searchTerm = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                FilteredChecks = Checks.Where(x => (!x.Complete && x.Authorised) || x.UpdatedAt.AddDays(7) > DateTime.Now).ToList();
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

        public void SendMessage()
        {
            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"q{SelectedCheck.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            _ = DatabaseHelper.Insert(newNote);

            List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.CanApproveRequests).Select(x => x.UserName));
            otherUsers.Add(App.NotificationsManager.users.Find(x => x.UserName == SelectedCheck.RaisedBy).UserName);

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, $"QC: {SelectedCheck.Product}", $"{App.CurrentUser.FirstName} left a comment: {NewMessage}");
                _ = DatabaseHelper.Insert(newNotification);
            }

            Notes.Add(newNote);
            LoadCheck();

            NewMessage = "";
        }
    }
}
