using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class DrawingBrowserViewModel : BaseViewModel, IRefreshableViewModel
    {
        public List<TechnicalDrawing> Drawings { get; set; }
        public List<TechnicalDrawing> FilteredDrawings { get; set; }

        private TechnicalDrawing selectedDrawing;
        public TechnicalDrawing SelectedDrawing
        {
            get { return selectedDrawing; }
            set
            {
                selectedDrawing = value;
                DisplayDrawing(value);
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

        public Uri FilePath { get; set; }

        private bool onlyShowCurrentDrawings;
        public bool OnlyShowCurrentDrawings
        {
            get { return onlyShowCurrentDrawings; }
            set
            {
                onlyShowCurrentDrawings = value;
                OnPropertyChanged();
                FilterDrawings();
            }
        }

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
            onlyShowCurrentDrawings = true;
            EditControlsVis = App.CurrentUser.CanCreateSpecial ? Visibility.Visible : Visibility.Collapsed;
            Drawings = new();
            FilteredDrawings = new();
        }

        private void LoadData()
        {
            Drawings.Clear();
            Drawings = DatabaseHelper.Read<TechnicalDrawing>();

            Drawings = Drawings.OrderBy(d => d.DrawingName).ThenBy(d => d.Revision).ToList();

            for (int i = 0; i < Drawings.Count; i++)
            {
                if (i < Drawings.Count - 1)
                {
                    Drawings[i].IsCurrent = (Drawings[i].DrawingName != Drawings[i + 1].DrawingName);
                }
                else
                {
                    Drawings[i].IsCurrent = true;
                }
            }
        }

        private void FilterDrawings(string searchString = "")
        {
            FilteredDrawings = null; // force UI refresh
            FilteredDrawings = new();
            if (string.IsNullOrEmpty(searchString))
            {
                FilteredDrawings = OnlyShowCurrentDrawings
                    ? Drawings.Where(d => d.IsCurrent).ToList()
                    : Drawings.ToList();
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

                if (SelectedDrawing == null)
                {
                    SelectedDrawing = FilteredDrawings[0];
                }
            }

            OnPropertyChanged(nameof(SelectedDrawingDisplayVis));
            OnPropertyChanged(nameof(NoneFoundVis));
            OnPropertyChanged(nameof(FilteredDrawings));
        }

        public void DisplayDrawing(TechnicalDrawing drawing)
        {
            if (drawing == null) return;

            ArchetypeWarningVis = drawing.IsArchetype
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(ArchetypeWarningVis));
        }

        public void AddNewDrawing()
        {
            AddNewDrawingWindow window = new(Drawings);
            window.ShowDialog();
            if (window.SaveExit)
            {
                Refresh();
                try
                {
                    SelectedDrawing = FilteredDrawings.Single(d => d.DrawingName == window.NewDrawing.DrawingName && d.Revision == window.NewDrawing.Revision);
                }
                catch
                {
                    SelectedDrawing = FilteredDrawings[0];
                }
            }
        }

        public void Refresh()
        {
            LoadData();
            FilterDrawings();
        }
    }
}
