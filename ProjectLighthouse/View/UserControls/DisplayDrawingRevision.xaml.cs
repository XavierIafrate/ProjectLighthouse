using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayDrawingRevision.xaml
    /// </summary>
    public partial class DisplayDrawingRevision : UserControl
    {


        public TechnicalDrawing Drawing
        {
            get { return (TechnicalDrawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Drawing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(TechnicalDrawing), typeof(DisplayDrawingRevision), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDrawingRevision control)
            {
                return;
            }

            control.DataContext = control.Drawing;
            string filePath = Path.Join(App.ROOT_PATH, control.Drawing.DrawingStore);
            if (!File.Exists(filePath))
            {
                control.openButton.Content = "file not found";
                control.openButton.IsEnabled = false;
            }
            else
            {
                control.openButton.Content = "Open";
                control.openButton.IsEnabled = true;
            }

            control.pendingControls.Visibility = (!control.Drawing.IsApproved && !control.Drawing.IsRejected) 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            control.approvedControls.Visibility = control.Drawing.IsApproved
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.rejectedControls.Visibility = control.Drawing.IsRejected
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.approvalControls.Visibility = (!control.Drawing.IsApproved && !control.Drawing.IsRejected && App.CurrentUser.GetFullName() != control.Drawing.CreatedBy)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public DisplayDrawingRevision()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.GetTempFileName() + ".pdf";
            File.Copy(Path.Join(App.ROOT_PATH, Drawing.DrawingStore), tmpPath);
            fileopener.StartInfo.Arguments = "\"" + tmpPath + "\"";
            _ = fileopener.Start();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            rejectButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text.Trim());
        }

        private void rejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainViewModel.SelectedViewModel is DrawingBrowserViewModel vm)
            {
                Drawing.RejectionReason = rejectionReason.Text.Trim();
                vm.RejectDrawing(Drawing);
            }
        }

        private void approveAmendButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainViewModel.SelectedViewModel is DrawingBrowserViewModel vm)
            {
                vm.ApproveAmendment(Drawing);
            }
        }

        private void approveRevButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainViewModel.SelectedViewModel is DrawingBrowserViewModel vm)
            {
                vm.ApproveRevision(Drawing);
            }
        }
    }
}
