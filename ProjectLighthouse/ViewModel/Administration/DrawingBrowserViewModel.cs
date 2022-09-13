using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class DrawingBrowserViewModel : BaseViewModel
    {
        public List<TechnicalDrawingGroup> DrawingGroups { get; set; }
        public List<TechnicalDrawingGroup> FilteredDrawingGroups { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
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


        private TechnicalDrawingGroup selectedGroup;
        public TechnicalDrawingGroup SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;

                if (selectedGroup == null)
                {
                    NoneFoundVis = Visibility.Visible;
                    DrawingVis = Visibility.Hidden;
                }
                else
                {
                    FilteredDrawings = selectedGroup.Drawings;
                    SelectedDrawing = FilteredDrawings.Last();
                    NoneFoundVis = Visibility.Hidden;
                    DrawingVis = Visibility.Visible;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredDrawings));
                OnPropertyChanged(nameof(NoneFoundVis));
                OnPropertyChanged(nameof(DrawingVis));
            }
        }

        private TechnicalDrawing selectedDrawing;

        public TechnicalDrawing SelectedDrawing
        {
            get { return selectedDrawing; }
            set
            {
                selectedDrawing = value;
                SetDrawingUi();
                OnPropertyChanged();
            }
        }

        public List<Note> SelectedDrawingNotes { get; set; }



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

        public Uri FilePath { get; set; }

        public Visibility NoneFoundVis { get; set; } = Visibility.Hidden;
        public Visibility DrawingVis { get; set; } = Visibility.Visible;

        public Visibility ArchetypeWarningVis { get; set; } = Visibility.Collapsed;
        public Visibility SelectedDrawingDisplayVis { get; set; } = Visibility.Visible;
        public Visibility EditControlsVis { get; set; } = Visibility.Collapsed;

        public Visibility ApprovalControlsVis { get; set; } = Visibility.Collapsed;
        public Visibility PendingApprovalVis { get; set; } = Visibility.Collapsed;
        public AddNewDrawingCommand AddNewCmd { get; set; }

        public OpenDrawingCommand OpenDrawingCmd { get; set; }
        public WithdrawDrawingCommand WithdrawCmd { get; set; }
        public AddCommentToDrawingCommand AddCommentToDrawingCmd { get; set; }

        private bool showOldGroups;

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


        public DrawingBrowserViewModel()
        {
            InitialiseVariables();
            LoadData();
            FilterDrawings();
        }

        private void InitialiseVariables()
        {
            EditControlsVis = App.CurrentUser.CanCreateSpecial ? Visibility.Visible : Visibility.Collapsed;

            Drawings = new();
            FilteredDrawings = new();
            SelectedDrawing = new();
            FilteredDrawingGroups = new();
            DrawingGroups = new();

            NewNoteText = "";

            AddNewCmd = new(this);
            OpenDrawingCmd = new(this);
            WithdrawCmd = new(this);
            AddCommentToDrawingCmd = new(this);
        }

        private void LoadData()
        {
            Drawings.Clear();
            Drawings = DatabaseHelper.Read<TechnicalDrawing>();
            Drawings = Drawings.OrderBy(d => d.DrawingName).ThenBy(d => d.Revision).ThenBy(x => x.Created).ToList();

            List<Note> Notes = DatabaseHelper.Read<Note>().ToList();
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
                    AllDrawingsWithdrawn = d.All(x => x.IsWithdrawn || x.IsRejected)
                };

                DrawingGroups.Add(newGroup);
            }

            if (DrawingGroups.Count > 0)
            {
                SelectedGroup = DrawingGroups[0];
            }
        }

        private void FilterDrawings(string searchString = "")
        {
            FilteredDrawings = null; // force UI refresh
            FilteredDrawings = new();
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

            if (FilteredDrawingGroups.Count > 0)
            {
                SelectedGroup = FilteredDrawingGroups[0];
            }
            else
            {
                SelectedGroup = null;
            }

            OnPropertyChanged(nameof(FilteredDrawingGroups));
            OnPropertyChanged(nameof(FilteredDrawings));
        }

        public void AddNewDrawing()
        {
            AddNewDrawingWindow window = new(Drawings);
            window.ShowDialog();
            if (window.SaveExit)
            {
                string DrawingName = window.NewDrawing.DrawingName;
                LoadData();
                SelectedGroup = DrawingGroups.Find(x => x.Name == DrawingName);
            }
        }

        private void SetDrawingUi()
        {
            if (selectedDrawing == null)
            {
                return;
            }
            string filePath = Path.Join(App.ROOT_PATH, selectedDrawing.DrawingStore);

            SelectedDrawingNotes = null;
            OnPropertyChanged(nameof(SelectedDrawingNotes));
            SelectedDrawingNotes = selectedDrawing.Notes;
            OnPropertyChanged(nameof(SelectedDrawingNotes));

            if (!File.Exists(filePath))
            {
                //control.openButton.Content = "file not found";
                //control.openButton.IsEnabled = false;
            }
            else
            {
                //control.openButton.Content = "Open";
                //control.openButton.IsEnabled = true;
            }
            PendingApprovalVis = !selectedDrawing.IsApproved && !selectedDrawing.IsRejected && !selectedDrawing.IsWithdrawn
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(PendingApprovalVis));

            ApprovalControlsVis = !selectedDrawing.IsApproved && !selectedDrawing.IsRejected && !selectedDrawing.IsWithdrawn && App.CurrentUser.CanApproveDrawings && selectedDrawing.CreatedBy != App.CurrentUser.GetFullName()
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
                DocumentReference = $"{SelectedDrawing.Id:0}",
                SentBy = App.CurrentUser.UserName,
                Message = newNoteText.Trim(),
                OriginalMessage = newNoteText.Trim(),
            };

            if (!DatabaseHelper.Insert(newNote))
            {
                return;
            }

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.CanApproveDrawings && x.GetFullName() != App.CurrentUser.GetFullName()).ToList();
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

        public void RejectDrawing(TechnicalDrawing drawing)
        {
            drawing.IsRejected = true;
            drawing.RejectedDate = DateTime.Now;
            drawing.RejectedBy = App.CurrentUser.GetFullName();
            DatabaseHelper.Update(drawing);

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.GetFullName() == drawing.CreatedBy).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Rejected: {drawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has rejected this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }

            int target = drawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));
        }

        public void ApproveRevision(TechnicalDrawing drawing)
        {
            TechnicalDrawingGroup group = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == drawing.Id));
            int MaxRev = group.Drawings.Max(x => x.Revision);

            drawing.Revision = MaxRev + 1;
            drawing.AmendmentType = TechnicalDrawing.Amendment.A;
            drawing.IsApproved = true;
            drawing.ApprovedDate = DateTime.Now;
            drawing.ApprovedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update(drawing);

            List<User> ToNotify = App.NotificationsManager.users.Where(x => x.GetFullName() == drawing.CreatedBy).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Approved: {drawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has approved this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }

            int target = drawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));

        }

        public void ApproveAmendment(TechnicalDrawing drawing)
        {
            TechnicalDrawingGroup group = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == drawing.Id));
            int MaxRev = group.Drawings.Max(x => x.Revision);
            TechnicalDrawing.Amendment maxAmd = group.Drawings.Max(x => x.AmendmentType);

            if (MaxRev == 0)
            {
                drawing.Revision = MaxRev + 1;
                drawing.AmendmentType = TechnicalDrawing.Amendment.A;
            }
            else if (maxAmd == Enum.GetValues(typeof(TechnicalDrawing.Amendment)).Cast<TechnicalDrawing.Amendment>().Max())
            {
                MessageBox.Show($"Amendment limit reached for Revision {MaxRev}, posting to new Revision", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);

                drawing.Revision = MaxRev + 1;
                drawing.AmendmentType = TechnicalDrawing.Amendment.A;
            }
            else
            {
                drawing.Revision = MaxRev;
                drawing.AmendmentType++;
            }
            drawing.IsApproved = true;
            drawing.ApprovedDate = DateTime.Now;
            drawing.ApprovedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update(drawing);
            int target = drawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));
        }

        public void OpenPdfDrawing()
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.Join(Path.GetTempPath(), selectedDrawing.GetSafeFileName());

            if (!File.Exists(tmpPath))
            {
                File.Copy(Path.Join(App.ROOT_PATH, selectedDrawing.DrawingStore), tmpPath);
            }
            fileopener.StartInfo.Arguments = "\"" + tmpPath + "\"";
            _ = fileopener.Start();
        }

        public void WithdrawDrawing()
        {
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
            string thisGroup = SelectedGroup.Name;
            int thisDrawing = SelectedDrawing.Id;

            DatabaseHelper.Update(SelectedDrawing);
            ShowOldGroups = true;
            LoadData();

            SelectedGroup = FilteredDrawingGroups.Find(x => x.Name == thisGroup);
            SelectedDrawing = SelectedGroup.Drawings.Find(x => x.Id == thisDrawing);

            MessageBox.Show($"{selectedDrawing.DrawingName} R{selectedDrawing.Revision}{selectedDrawing.AmendmentType} has been withdrawn for manufacture.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
