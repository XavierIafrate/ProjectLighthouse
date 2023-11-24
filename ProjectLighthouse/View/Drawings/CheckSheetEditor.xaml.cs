using DocumentFormat.OpenXml.Spreadsheet;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Quality.Internal;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Windows.Media.Streaming.Adaptive;

namespace ProjectLighthouse.View.Drawings
{
    public partial class CheckSheetEditor : Window
    {
        private TechnicalDrawing drawing;
        public List<ToleranceDefinition> Tolerances { get; set; }
        public List<Standard> Standards { get; set; }

        public List<ToleranceDefinition> AvailableTolerances { get; set; }
        public List<ToleranceDefinition?> ReferencedTolerances { get; set; }

        public CheckSheetEditor(List<ToleranceDefinition> dimensions, TechnicalDrawing drawing)
        {
            InitializeComponent();

            this.drawing = drawing;
            this.Tolerances = dimensions;
            ReferencedTolerances = new();

            foreach (string reference in drawing.Specification)
            {
                ReferencedTolerances.Add(Tolerances.Find(x => x.Id == reference));
            }

            AvailableTolerances = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();

            MasterToleranceListView.ItemsSource = AvailableTolerances;
            ReferencedTolerancesListView.ItemsSource = ReferencedTolerances;

            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances);
        }

        void RebuildCheckSheet()
        {
            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances);
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
            AvailableTolerances = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();
            ReferencedTolerancesListView.Items.Refresh();

            MasterToleranceListView.ItemsSource = null;
            MasterToleranceListView.ItemsSource = AvailableTolerances;

            RebuildCheckSheet();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MasterToleranceListView.SelectedValue is not ToleranceDefinition tol) return;
            List<string> tols = drawing.Specification;
            tols.Add(tol.Id);
            drawing.Specification = tols;
            ReferencedTolerances.Add(tol);
            AvailableTolerances = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();


            ReferencedTolerancesListView.Items.Refresh();
            MasterToleranceListView.ItemsSource = null;
            MasterToleranceListView.ItemsSource = AvailableTolerances;

            RebuildCheckSheet();
        }
    }
}
