using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.Model.Quality.Internal;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayTechnicalDrawing : UserControl
    {
        public TechnicalDrawing Drawing
        {
            get { return (TechnicalDrawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(TechnicalDrawing), typeof(DisplayTechnicalDrawing), new PropertyMetadata(null, SetValues));



        public string OrderReference
        {
            get { return (string)GetValue(OrderReferenceProperty); }
            set { SetValue(OrderReferenceProperty, value); }
        }

        public static readonly DependencyProperty OrderReferenceProperty =
            DependencyProperty.Register("OrderReference", typeof(string), typeof(DisplayTechnicalDrawing), new PropertyMetadata(null));

        public bool PlatingStatement
        {
            get { return (bool)GetValue(PlatingStatementProperty); }
            set { SetValue(PlatingStatementProperty, value); }
        }

        public static readonly DependencyProperty PlatingStatementProperty =
            DependencyProperty.Register("PlatingStatement", typeof(bool), typeof(DisplayTechnicalDrawing), new PropertyMetadata(false));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayTechnicalDrawing control) return;
            
            if (control.Drawing == null)
            {
                control.filename.Text = "<null>";
                control.rev.Text = "n/a";
                control.openButton.IsEnabled = false;
                control.inspectionLogButton.IsEnabled = false;
                return;
            }

            string filePath = Path.Join(App.ROOT_PATH, control.Drawing.DrawingStore);

            control.filename.Text = control.Drawing.DrawingName;
            control.rev.Text = control.Drawing.DrawingType == TechnicalDrawing.Type.Production
                ? $"Revision {control.Drawing.Revision}{control.Drawing.AmendmentType}"
                : $"Development v.{control.Drawing.Revision}{control.Drawing.AmendmentType}";
            control.issueDate.Text = control.Drawing.ApprovedDate == System.DateTime.MinValue ? "Issued [missing date]" : $"Issued {control.Drawing.ApprovedDate:dd/MM/yyyy}";
            control.newBadge.Visibility = control.Drawing.ApprovedDate.AddDays(7) > System.DateTime.Now
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.inspectionLogButton.Visibility = control.Drawing.Specification.Count > 0 ? Visibility.Visible : Visibility.Collapsed;


            if (!File.Exists(filePath))
            {
                control.openButton.Tag = "Not Found";
                control.openButton.IsEnabled = false;
            }
            else
            {
                control.openButton.Tag = "Drawing";
                control.openButton.IsEnabled = true;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.Join(Path.GetTempPath(), Drawing.GetSafeFileName());
            if (!File.Exists(tmpPath))
            {
                File.Copy(Path.Join(App.ROOT_PATH, Drawing.DrawingStore), tmpPath);
            }

            if (PlatingStatement)
            {
                File.Copy(Path.Join(App.ROOT_PATH, Drawing.DrawingStore), tmpPath, overwrite: true);
                TechnicalDrawing.AddPlatingStatement(tmpPath);
            }

            fileopener.StartInfo.Arguments = "\"" + tmpPath + "\"";
            _ = fileopener.Start();
        }

        public DisplayTechnicalDrawing()
        {
            InitializeComponent();
        }

        private void InspectionLogButton_Click(object sender, RoutedEventArgs e)
        {
            DimensionCheckSheet checkSheet = new();

            List<ToleranceDefinition> referencedTolerances = DatabaseHelper.Read<ToleranceDefinition>().Where(x => Drawing.Specification.Contains(x.Id)).ToList();

            List<ToleranceDefinition> orderedReferences = new();
            foreach (string spec in Drawing.Specification)
            {
                ToleranceDefinition? t = referencedTolerances.Find(x => x.Id == spec);
                if (t is not null)
                {
                    orderedReferences.Add(t);
                }
            }


            string buildPath = $"{App.ROOT_PATH}lib\\checksheets\\{Drawing.DrawingName}_R{Drawing.Revision:0}{Drawing.AmendmentType}";
            if (OrderReference is not null)
            {
                buildPath += $"_{OrderReference}";
            }
            buildPath += $".pdf";

            checkSheet.BuildContent(Drawing, orderedReferences, OrderReference, buildPath);

            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + buildPath + "\"";
            _ = fileopener.Start();
        }
    }
}
