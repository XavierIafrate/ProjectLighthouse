using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.View.Drawings;
using ProjectLighthouse.ViewModel.Commands.Drawings;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
        public List<ToleranceDefinition> Tolerances { get; set; }
        public List<ToleranceDefinition?> Specification { get; set; }

        private List<Note> Notes;
        private List<ProductGroup> ProductGroups;
        private List<TurnedProduct> TurnedProducts;

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
                Search();
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
                Search();
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

        public Visibility AdminVis { get; set; }
        #endregion

        #region Commands
        public AddNewDrawingCommand AddNewCmd { get; set; }
        public OpenDrawingCommand OpenDrawingCmd { get; set; }
        public WithdrawDrawingCommand WithdrawCmd { get; set; }
        public AddCommentToDrawingCommand AddCommentToDrawingCmd { get; set; }
        public ApproveDrawingCommand ApproveDrawingCmd { get; set; }
        public RejectDrawingCommand RejectDrawingCmd { get; set; }
        public ConvertToDevelopmentCommand ConvertToDevelopmentCmd { get; set; }
        public EditSpecificationCommand EditSpecificationCmd { get; set; }
        #endregion

        #endregion

        #region Loading
        public DrawingBrowserViewModel()
        {
            InitialiseVariables();
            LoadData();
            Search();
        }

        private void InitialiseVariables()
        {
            EditControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveDrawings)
                ? Visibility.Visible
                : Visibility.Collapsed;

            Drawings = new();
            FilteredDrawings = new();
            FilteredDrawingGroups = new();
            DrawingGroups = new();
            Specification = new();

            NewNoteText = "";
            RejectionStatement = "";

            AddNewCmd = new(this);
            OpenDrawingCmd = new(this);
            WithdrawCmd = new(this);
            AddCommentToDrawingCmd = new(this);
            ApproveDrawingCmd = new(this);
            RejectDrawingCmd = new(this);
            ConvertToDevelopmentCmd = new(this);
            EditSpecificationCmd = new(this);

            AdminVis = App.CurrentUser.Role == UserRole.Administrator
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void LoadData()
        {
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>();
            Tolerances = DatabaseHelper.Read<ToleranceDefinition>().OrderBy(x => x.Id).ToList();

            Drawings = DatabaseHelper.Read<TechnicalDrawing>();
            Drawings = Drawings
                .OrderBy(d => d.DrawingName)
                .ThenBy(d => d.Revision)
                .ThenBy(x => x.Created)
                .ToList();

            Notes = DatabaseHelper.Read<Note>().ToList();

            List<int?> GroupIds = Drawings
                .Where(x => x.TurnedProductId is null && x.GroupId is not null)
                .Select(x => x.GroupId)
                .Distinct()
                .ToList();

            List<int?> ProductIds = Drawings
                .Where(x => x.TurnedProductId is not null)
                .Select(x => x.TurnedProductId)
                .Distinct()
                .ToList();

            DrawingGroups = new();
            foreach (int? groupId in GroupIds)
            {
                ProductGroup? group;
                if (groupId is null)
                {
                    continue;
                }
                else
                {
                    group = ProductGroups.Find(x => x.Id == groupId);
                }

                if (group is null)
                {
                    MessageBox.Show($"Failed to find group with ID '{(groupId == null ? groupId : "null")}'");
                    continue;
                }

                List<TechnicalDrawing> groupDrawings = Drawings.Where(x => x.GroupId == groupId && x.TurnedProductId is null).ToList();
                int maxRev = groupDrawings.Max(x => x.Revision);
                TechnicalDrawing.Amendment maxAmd = groupDrawings.Where(x => x.Revision == maxRev).Max(x => x.AmendmentType);
                bool leadHasCheckSheet = false;
                for (int j = 0; j < groupDrawings.Count; j++)
                {
                    if (groupDrawings[j].AmendmentType == maxAmd
                        && groupDrawings[j].Revision == maxRev
                        && groupDrawings[j].IsApproved
                        && !groupDrawings[j].IsWithdrawn)
                    {
                        groupDrawings[j].IsCurrent = true;
                        if (groupDrawings[j].Specification.Count > 0)
                        {
                            leadHasCheckSheet = true;
                        }
                    }
                }

                TechnicalDrawingGroup newGroup = new()
                {
                    Drawings = groupDrawings.OrderBy(x => x.Created).ToList(),
                    Name = group.Name,
                    CurrentRevision = maxRev,
                    LastIssue = groupDrawings.Max(x => x.ApprovedDate),
                    Amendment = maxAmd,
                    AllDrawingsWithdrawn = groupDrawings.All(x => x.IsWithdrawn || x.IsRejected),
                    IsArchetypeGroup = true,
                    HasCheckSheet = leadHasCheckSheet
                };

                if (newGroup.LastIssue == DateTime.MinValue)
                {
                    newGroup.LastIssue = null;
                }

                DrawingGroups.Add(newGroup);
            }

            foreach (int? productId in ProductIds)
            {
                if (productId is null) continue;

                TurnedProduct? turnedProduct = TurnedProducts.Find(x => x.Id == productId);

                if (turnedProduct is null)
                {
                    MessageBox.Show($"Failed to find turned product with ID '{productId}'");
                    continue;
                }


                List<TechnicalDrawing> groupDrawings = Drawings.Where(x => x.TurnedProductId == productId).ToList();
                int maxRev = groupDrawings.Max(x => x.Revision);
                TechnicalDrawing.Amendment maxAmd = groupDrawings.Where(x => x.Revision == maxRev).Max(x => x.AmendmentType);

                for (int j = 0; j < groupDrawings.Count; j++)
                {
                    if (groupDrawings[j].AmendmentType == maxAmd && groupDrawings[j].Revision == maxRev && groupDrawings[j].IsApproved && !groupDrawings[j].IsWithdrawn)
                    {
                        groupDrawings[j].IsCurrent = true;
                    }
                }

                TechnicalDrawingGroup newGroup = new()
                {
                    Drawings = groupDrawings.OrderBy(x => x.Created).ToList(),
                    Name = turnedProduct.ProductName,
                    CurrentRevision = maxRev,
                    LastIssue = groupDrawings.Max(x => x.ApprovedDate),
                    Amendment = maxAmd,
                    AllDrawingsWithdrawn = groupDrawings.All(x => x.IsWithdrawn || x.IsRejected),
                    IsArchetypeGroup = false
                };

                if (newGroup.LastIssue == DateTime.MinValue)
                {
                    newGroup.LastIssue = null;
                }

                DrawingGroups.Add(newGroup);
            }


            DrawingGroups = DrawingGroups
                .OrderBy(x => !x.Drawings.Any(x => x.PendingApproval()))
                .ThenBy(x => !x.IsArchetypeGroup)
                .ToList();
        }
        #endregion

        private void Search()
        {
            if (string.IsNullOrEmpty(SearchBoxText))
            {
                FilteredDrawingGroups = new(DrawingGroups);

                if (!ShowOldGroups)
                {
                    FilteredDrawingGroups = FilteredDrawingGroups
                        .Where(x => !x.AllDrawingsWithdrawn)
                        .ToList();
                }

                OnPropertyChanged(nameof(FilteredDrawingGroups));

                if (FilteredDrawingGroups.Count > 0)
                {
                    SelectedGroup = FilteredDrawingGroups[0];
                }
                else
                {
                    SelectedGroup = null;
                }

                return;
            }

            string searchToken = SearchBoxText.Trim().ToUpperInvariant();

            FilteredDrawingGroups = DrawingGroups
                .Where(x => x.Name.Contains(searchToken))
                .ToList();
            OnPropertyChanged(nameof(FilteredDrawingGroups));

            if (FilteredDrawingGroups.Count > 0)
            {
                SelectedGroup = FilteredDrawingGroups[0];
                if (!ShowRejected && FilteredDrawings.Count == 0)
                {
                    ShowRejected = true;
                }
            }
            else
            {
                SelectedGroup = null;
            }
        }

        private void LoadGroup()
        {
            if (selectedGroup is null)
            {
                SelectedDrawing = null;
                return;
            }

            if (!ShowRejected)
            {
                FilteredDrawings = selectedGroup.Drawings
                    .Where(x => !x.IsRejected)
                    .Where(x => !(x.IsWithdrawn && !x.IsApproved))
                    .ToList();

                if (FilteredDrawings.Count == 0)
                {
                    ShowRejected = true;
                    return;
                }
            }
            else
            {
                FilteredDrawings = selectedGroup.Drawings;
            }

            if (FilteredDrawings.Count > 0)
            {
                SelectedDrawing = FilteredDrawings.Last();
            }
            else
            {
                SelectedDrawing = null;
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
            Search();

            SelectedGroup = FilteredDrawingGroups.Find(x => x.Drawings.Any(d => d.Id == newId));
        }

        private void SetDrawingUi()
        {
            if (selectedDrawing == null)
            {
                return;
            }
            string filePath = Path.Join(App.ROOT_PATH, selectedDrawing.DrawingStore);

            SelectedDrawingNotes = Notes.Where(x => x.DocumentReference == $"{selectedDrawing.Id:0}").ToList();
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
                && (selectedDrawing.CreatedBy != App.CurrentUser.GetFullName() || selectedDrawing.WatermarkOnly)
                ? Visibility.Visible
                : Visibility.Collapsed;

            OnPropertyChanged(nameof(ApprovalControlsVis));

            Specification = new();
            List<string> baseSpec = selectedDrawing.Specification;
            foreach (string address in baseSpec)
            {
                Specification.Add(Tolerances.Find(x => x.Id == address));
            }

            OnPropertyChanged(nameof(Specification));
        }

        public void AddCommentToDrawing()
        {
            if (SelectedGroup is null) return;

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

            List<User> ToNotify = App.NotificationsManager.Users.Where(x => x.HasPermission(PermissionType.ApproveDrawings) && x.GetFullName() != App.CurrentUser.GetFullName()).ToList();
            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Comment: {SelectedDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} added a comment to this drawing.", toastAction: $"viewDrawing:{SelectedDrawing.Id}"));
            }

            string thisGroup = SelectedGroup.Name;
            int thisDrawing = SelectedDrawing.Id;

            LoadData();
            NewNoteText = "";
            SelectedGroup = FilteredDrawingGroups.Find(x => x.Name == thisGroup);

            if (SelectedGroup is null) return;
            SelectedDrawing = SelectedGroup.Drawings.Find(x => x.Id == thisDrawing);
        }

        public void RejectDrawing()
        {
            if (SelectedDrawing is null) return;

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

            List<User> ToNotify = App.NotificationsManager.Users.Where(x => x.GetFullName() == SelectedDrawing.CreatedBy).ToList();
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
            //ApproveDrawingWindow approvalWindow = new(SelectedDrawing, SelectedGroup);
            //approvalWindow.ShowDialog();
            if (SelectedGroup is null) return;
            if (SelectedDrawing is null) return;

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

        public void ConvertToDevelopment()
        {
            if (SelectedDrawing is null) return;
            if (SelectedDrawing.IsRejected)
            {
                MessageBox.Show("Drawing is rejected, cannot convert");
                return;
            }

            SelectedDrawing.DrawingType = TechnicalDrawing.Type.Research;
            DatabaseHelper.Update(SelectedDrawing);
            SelectedDrawing.PrepareMarkedPdf();

            int target = SelectedDrawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));

            MessageBox.Show("Success");
        }

        public void PublishDrawing()
        {
            if (SelectedGroup is null) return;
            if (SelectedDrawing is null) return;

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

            List<User> ToNotify = App.NotificationsManager.Users.Where(x => x.HasPermission(PermissionType.ApproveDrawings) && x.GetFullName() != App.CurrentUser.GetFullName()).ToList();
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
            if (SelectedDrawing is null)
            {
                return;
            }

            SelectedDrawing.CopyToAppData();
            SelectedDrawing.ShellOpen();
        }

        public void WithdrawDrawing()
        {
            if (SelectedDrawing is null)
            {
                return;
            }

            if (App.CurrentUser.Role < UserRole.Administrator)
            {
                MessageBox.Show("You do not have permission to withdraw drawings.", "Not available", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (SelectedGroup is null) return;
            SelectedDrawing = SelectedGroup.Drawings.Find(x => x.Id == thisDrawing);

            if (SelectedDrawing is null) return;
            MessageBox.Show($"{SelectedDrawing.DrawingName} R{SelectedDrawing.Revision}{SelectedDrawing.AmendmentType} has been withdrawn for manufacture.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void EditSpecification()
        {
            CheckSheetEditor editor = new(Tolerances, SelectedDrawing, null);
            editor.ShowDialog();

            if (!editor.SaveExit) return;
            if (SelectedDrawing is null) return;


            List<string> baseSpec = SelectedDrawing.Specification;
            Specification = new();

            foreach (string address in baseSpec)
            {
                Specification.Add(Tolerances.Find(x => x.Id == address));
            }

            OnPropertyChanged(nameof(Specification));

        }
    }
}
