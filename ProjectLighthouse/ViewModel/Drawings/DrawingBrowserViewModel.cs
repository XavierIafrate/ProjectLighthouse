using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.View;
using ProjectLighthouse.View.Drawings;
using ProjectLighthouse.View.HelperWindows;
using ProjectLighthouse.ViewModel.Commands.Drawings;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Drawings
{
    public class DrawingBrowserViewModel : BaseViewModel
    {
        #region Variables
        public List<TechnicalDrawingGroup> DrawingGroups { get; set; }
        public List<TechnicalDrawingGroup> FilteredDrawingGroups { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        public List<Note> SelectedDrawingNotes { get; set; }

        #region Full Properties
        private List<TechnicalDrawing> filteredDrawings;
        public List<TechnicalDrawing> FilteredDrawings
        {
            get { return filteredDrawings; }
            set
            {
                filteredDrawings = value;
                OnPropertyChanged();
            }
        }

        private TechnicalDrawingGroup? selectedGroup;
        public TechnicalDrawingGroup? SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                LoadGroup();
                OnPropertyChanged();
            }
        }


        private TechnicalDrawing? selectedDrawing;
        public TechnicalDrawing? SelectedDrawing
        {
            get { return selectedDrawing; }
            set
            {
                selectedDrawing = value;
                SetDrawingUi();
                OnPropertyChanged();
            }
        }

        private string searchBoxText;
        public string SearchBoxText
        {
            get { return searchBoxText; }
            set
            {
                searchBoxText = value;
                FilterDrawings(value);
                OnPropertyChanged();
            }
        }

        private string rejectionStatement;
        public string RejectionStatement
        {
            get { return rejectionStatement; }
            set 
            { 
                rejectionStatement = value; 
                OnPropertyChanged(); 
            }
        }

        private string newNoteText;
        public string NewNoteText
        {
            get { return newNoteText; }
            set
            {
                newNoteText = value;
                OnPropertyChanged();
            }
        }

        private bool showOldGroups;
        public bool ShowOldGroups
        {
            get { return showOldGroups; }
            set
            {
                showOldGroups = value;
                OnPropertyChanged();
                FilterDrawings(SearchBoxText);
            }
        }

        private bool showRejected;
        public bool ShowRejected
        {
            get { return showRejected; }
            set 
            {
                showRejected = value;
                LoadGroup();
                OnPropertyChanged(); 
            }
        }

        private bool openFileButtonEnabled;
        public bool OpenFileButtonEnabled
        {
            get { return openFileButtonEnabled; }
            set
            {
                openFileButtonEnabled = value;
                OpenFileButtonText = value 
                    ? "Open File" 
                    : "Not Found";
                OnPropertyChanged();
            }
        }

        private string openFileButtonText;
        public string OpenFileButtonText
        {
            get { return openFileButtonText; }
            set 
            { 
                openFileButtonText = value; 
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Visibilities
        public Visibility EditControlsVis { get; set; } = Visibility.Collapsed;
        public Visibility ApprovalControlsVis { get; set; } = Visibility.Collapsed;
        public Visibility PendingApprovalVis { get; set; } = Visibility.Collapsed;
        #endregion

        #region Commands
        public AddNewDrawingCommand AddNewCmd { get; set; }
        public OpenDrawingCommand OpenDrawingCmd { get; set; }
        public WithdrawDrawingCommand WithdrawCmd { get; set; }
        public AddCommentToDrawingCommand AddCommentToDrawingCmd { get; set; }
        public ApproveDrawingCommand ApproveDrawingCmd { get; set; }
        public RejectDrawingCommand RejectDrawingCmd { get; set; }
        #endregion

        #endregion

        #region Loading
        public DrawingBrowserViewModel()
        {
            InitialiseVariables();
            LoadData();
            FilterDrawings();
        }

        private void InitialiseVariables()
        {
            EditControlsVis = App.CurrentUser.HasPermission(PermissionType.CreateSpecial) 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            Drawings = new();
            FilteredDrawings = new();
            SelectedDrawing = new();
            FilteredDrawingGroups = new();
            DrawingGroups = new();

            NewNoteText = "";
            RejectionStatement = "";

            AddNewCmd = new(this);
            OpenDrawingCmd = new(this);
            WithdrawCmd = new(this);
            AddCommentToDrawingCmd = new(this);
            ApproveDrawingCmd = new(this);
            RejectDrawingCmd = new(this);
        }

        private void LoadData()
        {
            Drawings = DatabaseHelper.Read<TechnicalDrawing>();
            Drawings = Drawings
                .OrderBy(d => d.DrawingName)
                .ThenBy(d => d.Revision)
                .ThenBy(x => x.Created)
                .ToList();

            List<Note> Notes = DatabaseHelper.Read<Note>()
                .ToList();

            for (int i = 0; i < Drawings.Count; i++)
            {
                Drawings[i].Notes = Notes.Where(x => x.DocumentReference == Drawings[i].Id.ToString("0")).ToList() ?? new();
            }

            string[] drawingGroups = Drawings.Select(x => x.DrawingName).Distinct().ToArray();
            DrawingGroups = new();

            for (int i = 0; i < drawingGroups.Length; i++)
            {
                List<TechnicalDrawing> d = Drawings.Where(x => x.DrawingName == drawingGroups[i]).ToList();
                int maxRev = d.Max(x => x.Revision);
                TechnicalDrawing.Amendment maxAmd = d.Where(x => x.Revision == maxRev).Max(x => x.AmendmentType);

                for (int j = 0; j < d.Count; j++)
                {
                    if (d[j].AmendmentType == maxAmd && d[j].Revision == maxRev && d[j].IsApproved && !d[j].IsWithdrawn)
                    {
                        d[j].IsCurrent = true;
                    }
                }
                TechnicalDrawingGroup newGroup = new()
                {
                    Drawings = d.OrderBy(x => x.Created).ToList(),
                    Name = drawingGroups[i],
                    CurrentRevision = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.Revision),
                    LastIssue = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.ApprovedDate),
                    Amendment = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.AmendmentType),
                    AllDrawingsWithdrawn = d.All(x => x.IsWithdrawn || x.IsRejected),
                    IsArchetypeGroup = d.First().IsArchetype
                };

                DrawingGroups.Add(newGroup);
            }
        }
        #endregion

        private void FilterDrawings(string searchString = "")
        {
            if (string.IsNullOrEmpty(searchString))
            {
                FilteredDrawingGroups = new(DrawingGroups);
                if (!ShowOldGroups)
                {
                    FilteredDrawingGroups = FilteredDrawingGroups.Where(x => !x.AllDrawingsWithdrawn).ToList();
                }
            }
            else
            {
                searchString = searchString.Trim().ToUpperInvariant();

                FilteredDrawingGroups = DrawingGroups.Where(x => x.Name.Contains(searchString)).ToList();
            }

            FilteredDrawingGroups = FilteredDrawingGroups.OrderByDescending(x => x.IsArchetypeGroup).ThenBy(x => x.Name).ToList();

            if (FilteredDrawingGroups.Count > 0)
            {
                SelectedGroup = FilteredDrawingGroups[0];
            }
            else
            {
                SelectedGroup = null;
            }

            if (SelectedGroup is not null)
            {
                if (ShowRejected)
                {
                    FilteredDrawings = SelectedGroup.Drawings;
                }
                else
                {
                    FilteredDrawings = SelectedGroup.Drawings
                        .Where(x => !x.IsRejected && !x.IsWithdrawn)
                        .ToList();
                }
            }

            OnPropertyChanged(nameof(FilteredDrawingGroups));
            OnPropertyChanged(nameof(FilteredDrawings));
        }

        private void LoadGroup()
        {
            if (selectedGroup is null)
            {
                return;
            }

            if (!ShowRejected)
            {
                FilteredDrawings = selectedGroup.Drawings
                    .Where(x => !x.IsRejected && !x.IsWithdrawn)
                    .ToList();
            }
            else
            {
                FilteredDrawings = selectedGroup.Drawings;
            }

            if (FilteredDrawings.Count > 0)
            {
                SelectedDrawing = FilteredDrawings.Last();
            }

            OnPropertyChanged(nameof(FilteredDrawings));
        }

        public void AddNewDrawing()
        {
           AddNewDrawingWindow window = new(Drawings)
           {
               Owner = Application.Current.MainWindow
           };

            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int newId = window.NewDrawing.Id;
            LoadData();
            FilterDrawings();

            SelectedGroup = FilteredDrawingGroups.Find(x => x.Drawings.Any(d => d.Id == newId));
        }

        private void SetDrawingUi()
        {
            if (selectedDrawing == null)
            {
                return;
            }
            string filePath = Path.Join(App.ROOT_PATH, selectedDrawing.DrawingStore);

            // TODO Fix this
            SelectedDrawingNotes = null;
            OnPropertyChanged(nameof(SelectedDrawingNotes));
            SelectedDrawingNotes = selectedDrawing.Notes;
            OnPropertyChanged(nameof(SelectedDrawingNotes));

            OpenFileButtonEnabled = File.Exists(filePath);

            PendingApprovalVis = !selectedDrawing.IsApproved && !selectedDrawing.IsRejected && !selectedDrawing.IsWithdrawn
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(PendingApprovalVis));

            ApprovalControlsVis = !selectedDrawing.IsApproved
                && !selectedDrawing.IsRejected
                && !selectedDrawing.IsWithdrawn
                && App.CurrentUser.HasPermission(PermissionType.ApproveDrawings)
                && selectedDrawing.CreatedBy != App.CurrentUser.GetFullName()
                ? Visibility.Visible
                : Visibility.Collapsed;

            OnPropertyChanged(nameof(ApprovalControlsVis));
        }

        public void AddCommentToDrawing()
        {
            if (string.IsNullOrEmpty(newNoteText.Trim()))
            {
                return;
            }

            Note newNote = new()
            {
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"{SelectedDrawing!.Id:0}",
                SentBy = App.CurrentUser.UserName,
                Message = newNoteText.Trim(),
                OriginalMessage = newNoteText.Trim(),
            };

            if (!DatabaseHelper.Insert(newNote))
            {
                return;
            }

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.HasPermission(PermissionType.ApproveDrawings) && x.GetFullName() != App.CurrentUser.GetFullName()).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Comment: {SelectedDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} added a comment to this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }


            SelectedDrawing.Notes.Add(newNote);
            string thisGroup = SelectedGroup.Name;
            int thisDrawing = SelectedDrawing.Id;

            LoadData();
            NewNoteText = "";
            SelectedGroup = FilteredDrawingGroups.Find(x => x.Name == thisGroup);
            SelectedDrawing = SelectedGroup.Drawings.Find(x => x.Id == thisDrawing);
        }

        public void RejectDrawing()
        {
            if (string.IsNullOrEmpty(RejectionStatement.Trim()))
            {
                MessageBox.Show("You are required to enter a justification for rejecting a drawing", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            SelectedDrawing.RejectionReason = RejectionStatement.Trim();
            SelectedDrawing.IsRejected = true;
            SelectedDrawing.RejectedDate = DateTime.Now;
            SelectedDrawing.RejectedBy = App.CurrentUser.GetFullName();
            DatabaseHelper.Update(SelectedDrawing);

            SelectedDrawing.PrepareMarkedPdf();

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.GetFullName() == SelectedDrawing.CreatedBy).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Rejected: {SelectedDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has rejected this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }

            RejectionStatement = "";
            int target = SelectedDrawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));
        }

        public void ApproveDrawing()
        {
            int MaxRev = SelectedGroup.Drawings.Max(x => x.Revision);
            TechnicalDrawing.Amendment maxAmd = SelectedGroup.Drawings.Where(x => x.Revision == MaxRev).Max(x => x.AmendmentType);

            // No input required, only one move to make as first revision or max amendment per rev
            if (MaxRev == 0 || maxAmd == Enum.GetValues(typeof(TechnicalDrawing.Amendment)).Cast<TechnicalDrawing.Amendment>().Max())
            {
                SelectedDrawing.Revision = MaxRev + 1;
                SelectedDrawing.AmendmentType = TechnicalDrawing.Amendment.A;
                PublishDrawing();
                return;
            }

            DefineDrawingApprovalWindow window = new()
            {
                DrawingToApprove = SelectedDrawing,
                DrawingsInGroup = SelectedGroup.Drawings,
                Owner = Application.Current.MainWindow,
            };

            window.SetupInterface();
            window.ShowDialog();

            if (window.Confirmed)
            {
                PublishDrawing();
            }
        }

        public void PublishDrawing()
        {
            TechnicalDrawing beingReplaced = SelectedGroup.Drawings.Find(x => x.IsCurrent);
            if (beingReplaced != null)
            {
                beingReplaced.IsCurrent = false;
                beingReplaced.PrepareMarkedPdf();
            }
            SelectedDrawing.IsApproved = true;
            SelectedDrawing.IsCurrent = true;
            SelectedDrawing.ApprovedDate = DateTime.Now;
            SelectedDrawing.ApprovedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update(SelectedDrawing);

            SelectedDrawing.PrepareMarkedPdf();

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.HasPermission(PermissionType.ApproveDrawings) && x.GetFullName() != App.CurrentUser.GetFullName()).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Approved: {SelectedDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has approved this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }

            int target = SelectedDrawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));

        }

        public void OpenPdfDrawing()
        {
            // TODO Implement new drawing system
            //ApproveDrawingWindow approveDrawingWindow = new(SelectedDrawing, SelectedGroup);
            //approveDrawingWindow.ShowDialog();
            //return;

            if (SelectedDrawing is null)
            {
                return;
            }

            SelectedDrawing.CopyToAppData();
            SelectedDrawing.ShellOpen();
        }

        public void WithdrawDrawing()
        {
            if(SelectedDrawing is null)
            {
                return;
            }
         
            if (SelectedDrawing.IsRejected)
            {
                MessageBox.Show("You cannot withdraw this drawing, it has been rejected.", "Not available", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedDrawing.IsWithdrawn)
            {
                MessageBox.Show("You cannot withdraw this drawing, it has already been withdrawn.", "Not available", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to withdraw this drawing?", "Confirm proceed", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            SelectedDrawing.IsWithdrawn = true;
            SelectedDrawing.WithdrawnDate = DateTime.Now;
            SelectedDrawing.WithdrawnBy = App.CurrentUser.GetFullName();

            string thisGroup = SelectedGroup!.Name;
            int thisDrawing = SelectedDrawing.Id;

            if (!DatabaseHelper.Update(SelectedDrawing))
            {
                MessageBox.Show("Failed to update the database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedDrawing.PrepareMarkedPdf();
            ShowOldGroups = true;

            LoadData();

            SelectedGroup = FilteredDrawingGroups.Find(x => x.Name == thisGroup);
            SelectedDrawing = SelectedGroup.Drawings.Find(x => x.Id == thisDrawing);

            MessageBox.Show($"{SelectedDrawing.DrawingName} R{SelectedDrawing.Revision}{SelectedDrawing.AmendmentType} has been withdrawn for manufacture.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
