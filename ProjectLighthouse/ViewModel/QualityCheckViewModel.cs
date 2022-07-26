using Microsoft.Win32;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class QualityCheckViewModel : BaseViewModel
    {
        public List<QualityCheck> Checks { get; set; }
        public List<Note> Notes { get; set; }
        public List<Attachment> Attachments { get; set; }

        private List<Attachment> filteredAttachments;
        public List<Attachment> FilteredAttachments
        {
            get { return filteredAttachments; }
            set { filteredAttachments = value; OnPropertyChanged(); }
        }


        private List<Note> filteredNotes;
        public List<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set { filteredNotes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<QualityCheck> FilteredChecks { get; set; }
        //public List<QualityCheck> FilteredChecks
        //{
        //    get { return filteredChecks; }
        //    set 
        //    { 
        //        filteredChecks = value; 
        //        OnPropertyChanged(); 
        //    }
        //}

        private QualityCheck selectedCheck;
        public QualityCheck SelectedCheck
        {
            get { return selectedCheck; }
            set
            {
                selectedCheck = value;
                OnPropertyChanged();
                LoadCheck();

            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set { searchTerm = value.ToUpper().Trim(); FilterChecks(searchTerm); OnPropertyChanged(); }
        }

        private string newMessage;
        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged(); }
        }

        public bool NoCheckFound { get; set; }

        public SendMessageCommand SendMessageCommand { get; set; }
        public UploadEvidenceCommand UploadEvidenceCmd { get; set; }

        public QualityCheckViewModel()
        {
            FilteredNotes = new();
            FilteredChecks = new();
            FilteredAttachments = new();

            Checks = DatabaseHelper.Read<QualityCheck>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();
            Attachments = DatabaseHelper.Read<Attachment>().ToList();

            SendMessageCommand = new(this);
            UploadEvidenceCmd = new(this);
            FilterChecks();
        }


        void FilterChecks(string searchTerm = null)
        {
            FilteredChecks.Clear();
            List<QualityCheck> checks = new();
            if (string.IsNullOrEmpty(searchTerm))
            {
                //checks = Checks.Where(x => (!x.Complete) || x.UpdatedAt.AddDays(7) > DateTime.Now).ToList();                
                checks = Checks.ToList();
            }
            else
            {
                checks = Checks.Where(x => x.Product.ToUpper().Contains(searchTerm)).ToList();
            }

            for (int i = 0; i < checks.Count; i++)
            {
                FilteredChecks.Add(checks[i]);
            }

            OnPropertyChanged(nameof(FilteredChecks));

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
            FilteredAttachments = Attachments.Where(x => x.DocumentReference == "q" + SelectedCheck.Id).ToList();
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

        public void GetEvidenceUpload()
        {
            OpenFileDialog filePicker = new() 
            { 
                Filter = "Pdf Files (*.pdf)|*.pdf|Image Files (*.png, *.jpg)|*.png;*.jpg|Excel Workbooks (*.xlsx)|*.xlsx|Word Docs (*.docx)|*.docx", 
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            if(!(bool)filePicker.ShowDialog())
            {
                return;
            }

            string storeName = $@"lib\{Path.GetRandomFileName()}";
            File.Copy(filePicker.FileName, $"{App.ROOT_PATH}{storeName}");

            Attachment newAttachment = new()
            {
                DocumentReference = "q" + SelectedCheck.Id,
                AttachmentStore = storeName,
                Extension = Path.GetExtension(filePicker.FileName),
                FileName = Path.GetFileNameWithoutExtension(filePicker.FileName),
                Remark="",
            };

            DatabaseHelper.Insert(newAttachment);
        }

        public class UploadEvidenceCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            private QualityCheckViewModel viewModel;
            public UploadEvidenceCommand(QualityCheckViewModel viewModel)
            {
                this.viewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return App.CurrentUser.Role >= UserRole.Production;
            }

            public void Execute(object parameter)
            {
                viewModel.GetEvidenceUpload();
            }
        }
    }
}
