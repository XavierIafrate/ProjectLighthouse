using ProjectLighthouse.Model.Drawings;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Drawings
{
    public partial class ApproveDrawingWindow : Window, INotifyPropertyChanged
    {
        private TechnicalDrawing drawing;
        private TechnicalDrawingGroup group;

        public event PropertyChangedEventHandler PropertyChanged;



        public TechnicalDrawing SelectedDrawing { get; set; }
        public bool CanApproveSelected { get; set; }


        public ApproveDrawingWindow(TechnicalDrawing d, TechnicalDrawingGroup g)
        {
            InitializeComponent();

            drawing = d; // clone?
            group = g;
            Revisions.ItemsSource = group.Drawings;

            Revisions.SelectedValue = drawing;


        }

        private void LoadDrawing(TechnicalDrawing d)
        {
            SelectedDrawing = d;
            CanApproveSelected = d.Id == drawing.Id;

            OnPropertyChanged(nameof(SelectedDrawing));
            OnPropertyChanged(nameof(CanApproveSelected));

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
            try
            {
                d.CopyToAppData();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            DisplayFile(d.GetLocalPath());
        }

        private void DisplayFile(string url, bool reload = false)
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
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 2)
        //        WindowState = WindowState == WindowState.Normal
        //            ? WindowState.Maximized
        //            : WindowState.Normal;
        //}

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
