using DocumentFormat.OpenXml.InkML;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayNote : UserControl
    {
        public Note Note
        {
            get => (Note)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(DisplayNote), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNote control) return;

            if (control.Note == null) return;

            control.DataContext = control.Note;

            DateTime SentAt = DateTime.Parse(control.Note.DateSent);
            control.SentDateTextBlock.Text = GetDateString(SentAt);
            control.SentAtTextBlock.Text = SentAt.ToString("HH:mm");

            control.SentByTextBlock.Text = control.Note.SentBy;
            control.EditedAtTextBlock.Visibility = control.Note.IsEdited
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.JustifyMessage(right: control.Note.SentBy == App.CurrentUser.UserName);

            control.DevBadge.Visibility = control.Note.SentBy == "xav"
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.MetadataStackPanel.Visibility = control.Note.ShowHeader
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.SentDateTextBlock.Visibility = control.Note.ShowDateHeader
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.ControlGrid.Margin = control.Note.ShowHeader
                ? new Thickness(5, 5, 5, 5)
                : new Thickness(5, 0, 5, 5);

            control.EditControls.Visibility = control.Note.ShowEdit && App.CurrentUser.UserName == control.Note.SentBy
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.spacer.Visibility = control.Note.SpaceUnder
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.UserRoleBadge.Visibility = control.Note.UserDetails == null ? Visibility.Collapsed : Visibility.Visible;

            control.SetOriginalMessageVis();
        }

        private void JustifyMessage(bool right = false)
        {
            ControlGrid.ColumnDefinitions[0].Width = right ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            ControlGrid.ColumnDefinitions[2].Width = !right ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            MetadataStackPanel.HorizontalAlignment = right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            bg.CornerRadius = right ? new(10, 10, 0, 10) : new(10, 10, 10, 0);

            System.Windows.Media.Brush brush = (System.Windows.Media.Brush)Application.Current.Resources[right ? "Blue" : "Green"];
            System.Windows.Media.Brush bgBrush = (System.Windows.Media.Brush)Application.Current.Resources[right ? "BlueFaded" : "GreenFaded"];

            bg.Background = bgBrush;
            SentByTextBlock.Foreground = brush;
            SentAtTextBlock.Foreground = brush;
            UserRoleBadge.Foreground = brush;
            EditedAtTextBlock.Foreground = brush;
        
        }

        public DisplayNote()
        {
            InitializeComponent();
        }

        private static string GetDateString(DateTime date)
        {
            if (date.Date == DateTime.Today)
            {
                return "Today";
            }
            else if (date.Date == DateTime.Today.AddDays(-1))
            {
                return "Yesterday";
            }

            string suffix = GetDaySuffix(date.Day);
            return $"{date:dddd}, {date.Day}{suffix} {date:MMMM yyyy}";
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Hidden;

            EditBox.Visibility = Visibility.Visible;
            EditBox.IsEnabled = true;

            CancelButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;

            EditBox.Focus();
            EditBox.Select(EditBox.Text.Length, 0);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            if (MessageTextBlock.Text != Note.OriginalMessage)
            {
                MessageTextBlock.Text = Note.Message;
                Note.DateEdited = DateTime.Now.ToString("s");
                Note.IsEdited = true;

                DatabaseHelper.Update(Note);
                Note.NotifyEdited();
                SetOriginalMessageVis();
            }

            SaveButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void SetOriginalMessageVis()
        {
            OriginalMessage.Visibility = Note.IsEdited
                && App.CurrentUser.Role == UserRole.Administrator
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Text = Note.Message;
            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            SaveButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;
        }
    }
}
