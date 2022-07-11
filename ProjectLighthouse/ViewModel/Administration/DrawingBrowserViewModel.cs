using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class DrawingBrowserViewModel : BaseViewModel
    {
        public List<DrawingGroup> DrawingGroups { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        public List<TechnicalDrawing> FilteredDrawings { get; set; }

        private DrawingGroup selectedGroup;
        public DrawingGroup SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                FilteredDrawings = selectedGroup.Drawings;
                //DisplayDrawing(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredDrawings));
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

        public Uri FilePath { get; set; }

        public Visibility NoneFoundVis { get; set; } = Visibility.Hidden;
        public Visibility ArchetypeWarningVis { get; set; } = Visibility.Collapsed;
        public Visibility SelectedDrawingDisplayVis { get; set; } = Visibility.Visible;
        public Visibility EditControlsVis { get; set; } = Visibility.Collapsed;
        public AddNewDrawingCommand AddNewCmd { get; set; }
        public bool StopRefresh { get; set; } = false;

        public DrawingBrowserViewModel()
        {
            InitialiseVariables();
            LoadData();
            FilterDrawings();
        }

        private void InitialiseVariables()
        {
            AddNewCmd = new(this);
            EditControlsVis = App.CurrentUser.CanCreateSpecial ? Visibility.Visible : Visibility.Collapsed;
            Drawings = new();
            FilteredDrawings = new();
        }

        private void LoadData()
        {
            Drawings.Clear();
            Drawings = DatabaseHelper.Read<TechnicalDrawing>();

            Drawings = Drawings.OrderBy(d => d.DrawingName).ThenBy(d => d.Revision).ThenBy(x => x.Created).ToList();
            List<Note> Notes = DatabaseHelper.Read<Note>().ToList();
            for (int i = 0; i < Drawings.Count; i++)
            {
                Drawings[i].Notes = Notes.Where(x => x.DocumentReference == Drawings[i].Id.ToString("0")).ToList();
            }

            string[] drawingGroups = Drawings.Select(x => x.DrawingName).Distinct().ToArray();
            DrawingGroups = new();
            for (int i = 0; i < drawingGroups.Length; i++)
            {
                List<TechnicalDrawing> d = Drawings.Where(x => x.DrawingName == drawingGroups[i]).ToList();
                int maxRev = d.Max(x => x.Revision);
                TechnicalDrawing.Amendment maxAmd = d.Max(x => x.AmendmentType);

                for (int j = 0; j < d.Count; j++)
                {
                    if (d[j].AmendmentType == maxAmd && d[j].Revision == maxRev && d[j].IsApproved)
                    {
                        d[j].IsCurrent = true;
                    }
                }
                DrawingGroup newGroup = new()
                {
                    Drawings = d,
                    Name = drawingGroups[i],
                    CurrentRevision = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.Revision),
                    LastIssue = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.ApprovedDate),
                    Amendment = Drawings.Where(x => x.DrawingName == drawingGroups[i]).Max(x => x.AmendmentType),
                };

                DrawingGroups.Add(newGroup);
            }

            //CleanDrawings();
        }

        void CleanDrawings()
        {
            for (int i = 0; i < Drawings.Count; i++)
            {
                if (string.IsNullOrEmpty(Drawings[i].DrawingStore))
                {
                    string tmpPath = Path.GetTempFileName() + ".pdf";
                    File.Copy(Path.Join(App.ROOT_PATH, Drawings[i].URL), tmpPath);
                    Drawings[i].DrawingStore = tmpPath;
                    DatabaseHelper.Update(Drawings[i]);
                }

            }
        }

        private void FilterDrawings(string searchString = "")
        {
            FilteredDrawings = null; // force UI refresh
            FilteredDrawings = new();
            if (string.IsNullOrEmpty(searchString))
            {
                SelectedGroup = new()
                {
                    Drawings = Drawings.Where(x => x.DrawingName == Drawings.First().DrawingName).ToList(),
                    Name = Drawings.First().DrawingName,
                    CurrentRevision = Drawings.Where(x => x.DrawingName == Drawings.First().DrawingName).Max(x => x.Revision),
                    LastIssue = Drawings.Where(x => x.DrawingName == Drawings.First().DrawingName).Max(x => x.ApprovedDate),
                    Amendment = Drawings.Where(x => x.DrawingName == Drawings.First().DrawingName).Max(x => x.AmendmentType)
                };
            }
            else
            {
                searchString = searchString.Trim().ToUpperInvariant();
                for (int i = 0; i < Drawings.Count; i++)
                {
                    if (Drawings[i].DrawingName.ToUpperInvariant().Contains(searchString))
                    {
                        FilteredDrawings.Add(Drawings[i]);
                    }
                    else if (!string.IsNullOrEmpty(Drawings[i].Customer))
                    {
                        if (Drawings[i].Customer.ToUpperInvariant().Contains(searchString))
                        {
                            FilteredDrawings.Add(Drawings[i]);
                        }
                    }
                }
            }

            if (FilteredDrawings.Count == 0)
            {
                SelectedDrawingDisplayVis = Visibility.Hidden;
                NoneFoundVis = Visibility.Visible;
            }
            else
            {
                SelectedDrawingDisplayVis = Visibility.Visible;
                NoneFoundVis = Visibility.Hidden;

                //if (SelectedDrawing == null)
                //{
                //    SelectedDrawing = FilteredDrawings[0];
                //}
            }

            OnPropertyChanged(nameof(SelectedDrawingDisplayVis));
            OnPropertyChanged(nameof(NoneFoundVis));
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

        public void RejectDrawing(TechnicalDrawing drawing)
        {
            drawing.IsRejected = true;
            drawing.RejectedDate = DateTime.Now;
            drawing.RejectedBy = App.CurrentUser.GetFullName();
            DatabaseHelper.Update(drawing);
            int target = drawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));
        }

        public void ApproveRevision(TechnicalDrawing drawing)
        {
            DrawingGroup group = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == drawing.Id));
            int MaxRev = group.Drawings.Max(x => x.Revision);

            drawing.Revision = MaxRev + 1;
            drawing.AmendmentType = TechnicalDrawing.Amendment.A;
            drawing.IsApproved = true;
            drawing.ApprovedDate = DateTime.Now;
            drawing.ApprovedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update(drawing);
            int target = drawing.Id;
            LoadData();
            SelectedGroup = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == target));

        }

        public void ApproveAmendment(TechnicalDrawing drawing)
        {
            DrawingGroup group = DrawingGroups.Find(x => x.Drawings.Any(y => y.Id == drawing.Id));
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

        public class DrawingGroup
        {
            public string Name { get; set; }
            public DateTime LastIssue { get; set; }
            public int CurrentRevision { get; set; }
            public TechnicalDrawing.Amendment Amendment { get; set; }
            public List<TechnicalDrawing> Drawings { get; set; }
        }
    }
}
