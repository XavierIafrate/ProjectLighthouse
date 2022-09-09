using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayDrawingRevision : UserControl, INotifyPropertyChanged
    {


        public TechnicalDrawing Drawing
        {
            get { return (TechnicalDrawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Drawing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(TechnicalDrawing), typeof(DisplayDrawingRevision), new PropertyMetadata(null, SetValues));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


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

            control.approvalControls.Visibility = (!control.Drawing.IsApproved && !control.Drawing.IsRejected && App.CurrentUser.GetFullName() != control.Drawing.CreatedBy && App.CurrentUser.CanApproveDrawings)
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.notesDisplay.Notes = control.Drawing.Notes;
        }

        public DisplayDrawingRevision()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.Join(Path.GetTempPath(), Drawing.GetSafeFileName());

            if (!File.Exists(tmpPath))
            {
                File.Copy(Path.Join(App.ROOT_PATH, Drawing.DrawingStore), tmpPath);
            }
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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewNote(Message.Text.Trim());
        }

        private void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendButton.IsEnabled = !string.IsNullOrEmpty(Message.Text.Trim());
        }

        void AddNewNote(string note)
        {
            Note newNote = new()
            {
                Message = note,
                OriginalMessage = note,
                IsEdited = false,
                IsDeleted = false,
                SentBy = App.CurrentUser.UserName,
                DateSent = DateTime.Now.ToString("s"),
                DateEdited = DateTime.MinValue.ToString("s"),
                DocumentReference = Drawing.Id.ToString("0")
            };

            // people who have already commented
            List<string> toUpdate = Drawing.Notes.Select(x => x.SentBy).Distinct().ToList();

            List<string> otherUsers;

            otherUsers = DatabaseHelper.Read<User>().Where(x => x.CanApproveDrawings && x.ReceivesNotifications).Select(x => x.UserName).ToList();

            toUpdate.AddRange(otherUsers);
            toUpdate = toUpdate.Distinct().Where(x => x != App.CurrentUser.UserName).ToList();

            for (int i = 0; i < toUpdate.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: toUpdate[i], from: App.CurrentUser.UserName, header: $"Comment - {Drawing.DrawingName}", body: $"{App.CurrentUser.FirstName} left a comment on this drawing.", toastAction: $"viewDrawing:{Drawing.DrawingName}"));
            }

            DatabaseHelper.Insert(newNote);
            Drawing.Notes.Add(newNote);
            notesDisplay.Notes = null;
            notesDisplay.Notes = Drawing.Notes;

            Message.Text = "";
        }
    }
}
