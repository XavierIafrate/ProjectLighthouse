using ProjectLighthouse.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace View
{
    public partial class ApproveDrawingWindow : Window
    {
        private TechnicalDrawing drawing;
        private TechnicalDrawingGroup group;

        public ApproveDrawingWindow(TechnicalDrawing d, TechnicalDrawingGroup g)
        {
            InitializeComponent();

            // prevent from covering taskbar - no idea what the 12 is about lol
            this.MaxHeight = SystemParameters.WorkArea.Height+6;
            this.MaxWidth = SystemParameters.WorkArea.Width+6;


            drawing = d; // clone?
            group = g;
            Revisions.ItemsSource = group.Drawings;

            Revisions.SelectedValue = drawing;
        }

        private void LoadDrawing(TechnicalDrawing d)
        {
            //approval enable
            if (d.IsApproved)
            {
                tipText.Text = $"You are viewing Revision {d.Revision}{d.AmendmentType}";
                ApprovalControls.IsEnabled = false;
            }
            else if (d.IsRejected)
            {
                tipText.Text = $"You are viewing a rejected candidate.";
                ApprovalControls.IsEnabled = false;
            }
            else
            {
                tipText.Text = $"You are viewing Release Candidate #{d.Id}";
                ApprovalControls.IsEnabled = true;
            }
            d.CopyToAppData();
            DisplayFile(d.GetLocalPath());
        }

        private void DisplayFile(string url, bool reload=false)
        {
            System.Uri uri = new(url);
            webView.Source = uri;

            if (reload)
            {
                webView.Reload();
            }
        }

        private void Revisions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadDrawing((TechnicalDrawing)Revisions.SelectedValue);
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                this.WindowState = this.WindowState == WindowState.Normal
                    ? WindowState.Maximized
                    : WindowState.Normal;
        }
    }
}
