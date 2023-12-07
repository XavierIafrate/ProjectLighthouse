using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Quality.Internal;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Drawings
{
    public partial class CheckSheetEditor : Window
    {
        private TechnicalDrawing drawing;
        public List<ToleranceDefinition> Tolerances { get; set; }
        public List<Standard> Standards { get; set; }

        private Standard? selectedStandard;

        public Standard? SelectedStandard
        {
            get { return selectedStandard; }
            set { selectedStandard = value; FilterAvailable(); }
        }

        public List<ToleranceDefinition> AvailableTolerances { get; set; }
        public List<ToleranceDefinition?> ReferencedTolerances { get; set; }
        private string? order;

        public CheckSheetEditor(List<ToleranceDefinition> dimensions, TechnicalDrawing drawing, string? orderReference)
        {
            InitializeComponent();


            string testJson = File.ReadAllText(@"C:\Users\x.iafrate\Desktop\fitTest.txt");
            App.StandardFits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StandardFit>>(testJson);


            this.order = orderReference;

            this.drawing = drawing;
            this.Tolerances = dimensions;
            ReferencedTolerances = new();
            Standards = DatabaseHelper.Read<Standard>().OrderBy(x => x.Name).Prepend(null).ToList();

            foreach (ToleranceDefinition definition in Tolerances)
            {
                definition.standard = Standards.Find(x => x?.Id == definition.StandardId);
            }

            foreach (string reference in drawing.Specification)
            {
                ReferencedTolerances.Add(Tolerances.Find(x => x.Id == reference));
            }


            AvailableTolerances = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();

            MasterToleranceListView.ItemsSource = AvailableTolerances;
            ReferencedTolerancesListView.ItemsSource = ReferencedTolerances;


            StandardsFilter.ItemsSource = Standards;
            StandardsFilter.SelectedIndex = 0;

            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances, order);
        }

        void FilterAvailable()
        {
            List<ToleranceDefinition> tols = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();
            if (SelectedStandard is not null)
            {
                tols = tols.Where(x => x.StandardId == SelectedStandard.Id).ToList();
            }

            AvailableTolerances = tols;

            MasterToleranceListView.ItemsSource = null;
            MasterToleranceListView.ItemsSource = AvailableTolerances;
        }

        void RebuildCheckSheet()
        {
            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances, order);
            webView.Reload();
        }

        private void MasterToleranceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddButton.IsEnabled = MasterToleranceListView.SelectedValue is not null;
        }

        private void ReferencedTolerancesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveButton.IsEnabled = ReferencedTolerancesListView.SelectedValue is not null;

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReferencedTolerancesListView.SelectedValue is not ToleranceDefinition tol) return;
            List<string> tols = drawing.Specification;
            tols.Remove(tol.Id);
            drawing.Specification = tols;
            ReferencedTolerances.Remove(tol);
            ReferencedTolerancesListView.Items.Refresh();

            FilterAvailable();

            RebuildCheckSheet();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterToleranceListView.SelectedValue is not ToleranceDefinition tol) return;
            List<string> tols = drawing.Specification;
            tols.Add(tol.Id);
            drawing.Specification = tols;
            ReferencedTolerances.Add(tol);

            ReferencedTolerancesListView.Items.Refresh();
            FilterAvailable();

            RebuildCheckSheet();
        }

        private void ReferencedTolerancesListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not DisplayToleranceDefinition item) return;

            DragDrop.DoDragDrop(item, item.Tolerance, DragDropEffects.Move);
            ReferencedTolerancesListView.SelectedIndex = ReferencedTolerances.IndexOf(ReferencedTolerances.Find(x => x.Id == item.Tolerance.Id));
        }

        private void ReferencedTolerancesListView_Drop(object sender, DragEventArgs e)
        {
            ToleranceDefinition droppedData = e.Data.GetData(typeof(ToleranceDefinition)) as ToleranceDefinition;
            ToleranceDefinition target = ((DisplayToleranceDefinition)(sender)).Tolerance;

            int removedIdx = ReferencedTolerancesListView.Items.IndexOf(droppedData);
            int targetIdx = ReferencedTolerancesListView.Items.IndexOf(target);

            if (removedIdx == targetIdx)
                return;

            if (removedIdx < targetIdx)
            {
                ReferencedTolerances.Insert(targetIdx + 1, droppedData);
                ReferencedTolerances.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (ReferencedTolerances.Count + 1 > remIdx)
                {
                    ReferencedTolerances.Insert(targetIdx, droppedData);
                    ReferencedTolerances.RemoveAt(remIdx);
                }
            }

            ReferencedTolerancesListView.Items.Refresh();
            RebuildCheckSheet();
        }

        private void StandardsFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStandard = StandardsFilter.SelectedValue as Standard;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            string clipboardText= Clipboard.GetText();


            List<string>? references;

            try
            {
                references = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(clipboardText);
            }
            catch
            {
                MessageBox.Show("Not in valid format");
                return;
            }

            if (references is null)
            {
                MessageBox.Show("Not in valid format");
                return;
            }

            List<ToleranceDefinition> newTols = new();
            List<string> notFoundRefs = new();
            foreach(string reference in references)
            {
                ToleranceDefinition? t = Tolerances.Find(x => x.Id == reference);
                if (t is null)
                {
                    notFoundRefs.Add(reference);
                }
                else
                {
                    newTols.Add(t);
                }
            }

            if (notFoundRefs.Count > 0)
            {
                MessageBox.Show($"{notFoundRefs.Count} references not found, not proceeding");
                return;
            }

            drawing.Specification = references;
            ReferencedTolerances.Clear();
            foreach (string reference in drawing.Specification)
            {
                ReferencedTolerances.Add(Tolerances.Find(x => x.Id == reference));
            }
            ReferencedTolerancesListView.Items.Refresh();
            FilterAvailable();
            RebuildCheckSheet();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string serialisedReferences = Newtonsoft.Json.JsonConvert.SerializeObject(
                ReferencedTolerances.Where(x => x is not null).Select(x => x!.Id).ToList());
            Clipboard.SetText(serialisedReferences);
        }
    }
}
