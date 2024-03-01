using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Quality.Internal;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        List<TreeViewItem> mainToleranceList;
        public List<ToleranceDefinition> AvailableTolerances { get; set; }
        public List<ToleranceDefinition> ReferencedTolerances { get; set; }
        private string? order;
        private string buildPath;
        public bool SaveExit;

        bool webviewNavCompleted;
        bool webviewNeedsReload;

        public CheckSheetEditor(List<ToleranceDefinition> dimensions, TechnicalDrawing drawing, string? orderReference)
        {
            InitializeComponent();

            this.order = orderReference;

            this.drawing = drawing;
            this.Tolerances = dimensions;
            ReferencedTolerances = new();
            Standards = DatabaseHelper.Read<Standard>().OrderBy(x => x.Name).ToList(); //Prepend(null).

            buildPath = Path.GetTempPath() + "checksheet.pdf";


            foreach (ToleranceDefinition definition in Tolerances)
            {
                definition.standard = Standards.Find(x => x?.Id == definition.StandardId);
            }

            foreach (string reference in drawing.Specification)
            {
                ReferencedTolerances.Add(Tolerances.Find(x => x.Id == reference));
            }

            ReferencedTolerancesListView.ItemsSource = ReferencedTolerances;

            SetMasterTolerances();

            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances, order, buildPath);
        }

        private void SetMasterTolerances()
        {
            mainToleranceList = new();
            IEnumerable<IGrouping<int, ToleranceDefinition>> tolerancesByStandard = Tolerances.GroupBy(x => x.StandardId);

            for (int i = 0; i < tolerancesByStandard.Count(); i++)
            {
                IGrouping<int, ToleranceDefinition> group = tolerancesByStandard.ElementAt(i);

                Standard? standard = Standards.Find(x => x.Id == group.Key);
                TextBlock t = new() 
                { 
                    Text = (standard == null ? "No Standard" : $"{standard.Name} ({standard.Description})"),
                    TextTrimming=TextTrimming.CharacterEllipsis,
                    FontWeight=FontWeights.SemiBold,
                    FontSize=14
                };

                TreeViewItem standardItem = new() { Header = t };
                List<TreeViewItem> nameGroups = new();

                IEnumerable<IGrouping<string, ToleranceDefinition>> tolerancesByName = group.GroupBy(x => x.Name);
                

                for (int j = 0; j < tolerancesByName.Count(); j++)
                {
                    IGrouping<string, ToleranceDefinition> nameGroup = tolerancesByName.ElementAt(j);
                    TextBlock featureHeader = new()
                    {
                        Text = nameGroup.Key,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        FontStyle = FontStyles.Italic,
                        FontSize = 13
                    };
                    TreeViewItem nameItem = new() { Header =  featureHeader };
                    List<TreeViewItem> nameItems = new();
                    nameGroup.ToList().ForEach(x =>
                    {
                        Grid contentGrid = new();
                        contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                        contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

                        Button addButton = new()
                        {
                            Style = (Style)Application.Current.Resources["Action_New_Button_Small"],
                            CommandParameter = x,
                        };

                        DisplayToleranceDefinition displayToleranceDefinition = new() { Tolerance = x };

                        contentGrid.Children.Add(displayToleranceDefinition);
                        contentGrid.Children.Add(addButton);

                        Grid.SetColumn(addButton, 1);


                        addButton.Click += new RoutedEventHandler(AddToSheetButton_Click);

                        nameItems.Add(new() 
                        {
                            Header = contentGrid,
                            HorizontalContentAlignment =HorizontalAlignment.Stretch,
                        });
                    });
                    nameItem.ItemsSource = nameItems;
                    nameGroups.Add(nameItem);
                }
                standardItem.ItemsSource = nameGroups;
                mainToleranceList.Add(standardItem);
            }

            MasterTolerances.ItemsSource = mainToleranceList;
        }

        private void AddToSheetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.CommandParameter is not ToleranceDefinition tolerance) return;

            //if (MasterToleranceListView.SelectedValue is not ToleranceDefinition tol) return;
            List<string> tols = drawing.Specification;
            tols.Add(tolerance.Id);
            drawing.Specification = tols;
            ReferencedTolerances.Add(tolerance);

            ReferencedTolerancesListView.Items.Refresh();
            //FilterAvailable();

            RebuildCheckSheet();

            button.IsEnabled = false;
        }

        void FilterAvailable()
        {
            List<ToleranceDefinition> tols = Tolerances.Where(x => !drawing.Specification.Contains(x.Id)).ToList();
            if (SelectedStandard is not null)
            {
                tols = tols.Where(x => x.StandardId == SelectedStandard.Id).ToList();
            }

            AvailableTolerances = tols;

            //MasterToleranceListView.ItemsSource = null;
            //MasterToleranceListView.ItemsSource = AvailableTolerances;
        }

        void RebuildCheckSheet()
        {
            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(drawing, ReferencedTolerances, order, buildPath);

            if (!webviewNavCompleted)
            {
                webviewNeedsReload = true;
                return;
            }

            webviewNavCompleted = false;
            webView.Reload();
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

    

        private void ReferencedTolerancesListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not DisplayToleranceDefinition item) return;

            DragDrop.DoDragDrop(item, item.Tolerance, DragDropEffects.Move);
            ReferencedTolerancesListView.SelectedIndex = ReferencedTolerances.IndexOf(ReferencedTolerances.Find(x => x.Id == item.Tolerance.Id));
        }

        private void ReferencedTolerancesListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ToleranceDefinition)) is not ToleranceDefinition droppedData) return;
            if (sender is not DisplayToleranceDefinition droppedOnItem) return;
            ToleranceDefinition target = droppedOnItem.Tolerance;

            if (droppedData.Id == target.Id)
            {
                ClearDragFormats();
                return;
            }

            // Drop on top or bottom of control
            Point position = e.GetPosition(droppedOnItem);
            double height = droppedOnItem.ActualHeight;
            bool bottom = position.Y > height / 2;

            int removedIdx = ReferencedTolerancesListView.Items.IndexOf(droppedData);
            int targetIdx = ReferencedTolerancesListView.Items.IndexOf(target);

            if (bottom) targetIdx++;

            if (targetIdx == removedIdx)
            {
                ClearDragFormats();
                return;
            }
            else if (removedIdx >= targetIdx)
            {
                removedIdx--;
                if (targetIdx == removedIdx)
                {
                    ClearDragFormats();
                    return;
                }
            }


            if (removedIdx < targetIdx)
            {
                ReferencedTolerances.Insert(targetIdx, droppedData);
                ReferencedTolerances.RemoveAt(removedIdx);
            }
            else
            {
                if (ReferencedTolerances.Count + 1 > removedIdx)
                {
                    ReferencedTolerances.Insert(targetIdx, droppedData);
                    ReferencedTolerances.RemoveAt(removedIdx);
                }
            }

            ReferencedTolerancesListView.Items.Refresh();
            RebuildCheckSheet();
        }


        void ClearDragFormats()
        {
            for (int i = 0; i < ReferencedTolerancesListView.Items.Count; i++)
            {
                foreach (DisplayToleranceDefinition control in FindVisualChildren<DisplayToleranceDefinition>(ReferencedTolerancesListView))
                {
                    control.RemoveDragFormats();
                }
            }
        }

        private void StandardsFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedStandard = StandardsFilter.SelectedValue as Standard;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            string clipboardText = Clipboard.GetText();


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
            foreach (string reference in references)
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            drawing.Specification = ReferencedTolerances.Select(x => x.Id).ToList();
            try
            {
                DatabaseHelper.Update(drawing, throwErrs: true);
                SaveExit = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Close();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is not null and T t)
                {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.webView.Source = new Uri($"file:///{buildPath}#toolbar=0");

            await webView.EnsureCoreWebView2Async();
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            webviewNavCompleted = true;

            if (webviewNeedsReload)
            {
                webviewNavCompleted = false;
                webView.Reload();
            }
        }

        private void AddToleranceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            MessageBox.Show(btn.Tag.ToString());
        }
    }
}
