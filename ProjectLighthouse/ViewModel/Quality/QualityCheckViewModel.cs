using Microsoft.Win32;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.View.Quality;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Quality;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Quality
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
        public AddQualityCheckCommand AddNewCheckCommand { get; set; }
        public ResolveSampleCommand ResolveCommand { get; set; }


        public bool EvidenceUploaded { get; set; }
        public bool IsModified { get; set; }
        public bool CanApprove { get; set; }
        public bool NoResult { get; set; }
        public bool Incomplete { get; set; }
        public bool CanAddCheck { get; set; }

        public QualityCheckViewModel()
        {
            FilteredNotes = new();
            FilteredChecks = new();
            FilteredAttachments = new();

            SendMessageCommand = new(this);
            UploadEvidenceCmd = new(this);
            AddNewCheckCommand = new(this);
            ResolveCommand = new(this);

            CanAddCheck = App.CurrentUser.Role == UserRole.Purchasing || App.CurrentUser.Role >= UserRole.Scheduling;

            LoadData();
        }

        void LoadData()
        {
            Checks = DatabaseHelper.Read<QualityCheck>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();
            Attachments = DatabaseHelper.Read<Attachment>().ToList();

            SearchTerm = "";
            FilterChecks();
        }


        void FilterChecks(string searchTerm = null)
        {
            FilteredChecks.Clear();
            List<QualityCheck> checks = new();
            if (string.IsNullOrEmpty(searchTerm))
            {
                checks = Checks.OrderByDescending(x => x.RaisedAt).ToList();
            }
            else
            {
                checks = Checks.Where(x => x.Product.ToUpper().Contains(searchTerm) || $"VIEWQC:{x.Id}" == searchTerm).OrderByDescending(x => x.RaisedAt).ToList();
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

            IsModified = SelectedCheck.UpdatedAt != DateTime.MinValue;
            OnPropertyChanged(nameof(IsModified));

            CanApprove = SelectedCheck.Status == "Pending" && App.CurrentUser.Role >= UserRole.Production;
            OnPropertyChanged(nameof(CanApprove));

            EvidenceUploaded = FilteredAttachments.Count > 0;
            OnPropertyChanged(nameof(EvidenceUploaded));

            NoResult = SelectedCheck.Status == "Pending" && !CanApprove;
            OnPropertyChanged(nameof(NoResult));

            Incomplete = SelectedCheck.Status == "Pending";
            OnPropertyChanged(nameof(Incomplete));
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
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasQualityNotifications).Select(x => x.UserName));
            otherUsers.Add(App.NotificationsManager.users.Find(x => x.UserName == SelectedCheck.RaisedBy).UserName);

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, $"QC: {SelectedCheck.Product}", $"{App.CurrentUser.FirstName} left a comment: {NewMessage}", toastAction: $"viewQC:{SelectedCheck.Id}");
                _ = DatabaseHelper.Insert(newNotification);
            }

            Notes.Add(newNote);
            UpdateCurrentCheck();

            NewMessage = "";
        }

        public void GetEvidenceUpload()
        {
            OpenFileDialog filePicker = new()
            {
                Filter = "Pdf Files (*.pdf)|*.pdf|Image Files (*.png, *.jpg)|*.png;*.jpg|Excel Workbooks (*.xlsx)|*.xlsx|Word Docs (*.docx)|*.docx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            if (!(bool)filePicker.ShowDialog())
            {
                return;
            }

            Attachment newAttachment = new()
            {
                DocumentReference = "q" + SelectedCheck.Id,
                Remark = "",
            };

            newAttachment.CopyToStore(filePicker.FileName);

            DatabaseHelper.Insert(newAttachment);

            UpdateCurrentCheck();
        }

        void UpdateCurrentCheck()
        {
            SelectedCheck.UpdatedAt = DateTime.Now;
            SelectedCheck.UpdatedBy = App.CurrentUser.UserName;
            DatabaseHelper.Update(SelectedCheck);
            int checkId = SelectedCheck.Id;
            LoadData();
            SelectedCheck = Checks.Find(x => x.Id == checkId);
        }

        public void AddQualityCheck()
        {
            RequestNewQualityCheckWindow window = new();
            window.ShowDialog();
            if (window.RequestAdded)
            {
                LoadData();
            }
        }

        public void MarkCheckResolved(bool approved = false)
        {
            if (filteredAttachments.Count == 0)
            {
                MessageBox.Show("You are required to upload evidence before posting a result.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedCheck.IsAccepted = approved;
            SelectedCheck.IsRejected = !approved;
            SelectedCheck.CheckedAt = DateTime.Now;
            SelectedCheck.CheckedBy = App.CurrentUser.UserName;

            UpdateCurrentCheck();

            List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasQualityNotifications).Select(x => x.UserName));
            otherUsers.Add(App.NotificationsManager.users.Find(x => x.UserName == SelectedCheck.RaisedBy).UserName);

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();
            string title = approved ? "Accepted" : "Rejected";
            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, $"{title}: {SelectedCheck.Product}", $"This quality check request has been resolved", toastAction: $"viewQC:{SelectedCheck.Id}");
                _ = DatabaseHelper.Insert(newNotification);
            }
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

        public class ResolveSampleCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private QualityCheckViewModel viewModel;

            public ResolveSampleCommand(QualityCheckViewModel viewModel)
            {
                this.viewModel = viewModel;
            }
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is not string p) throw new Exception("need boolean string in"); ;

                if (bool.TryParse(p, out bool approve))
                {
                    viewModel.MarkCheckResolved(approve);
                }
                else
                {
                    throw new Exception("need boolean in");
                }
            }
        }
    }
}
