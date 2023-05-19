using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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


        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(DisplayNote), new PropertyMetadata(null, SetDelete));

        private static void SetDelete(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNote control) return;
            control.DeleteButton.IsEnabled = control.DeleteCommand is not null;
        }


        public ICommand SaveEditCommand
        {
            get { return (ICommand)GetValue(SaveEditCommandProperty); }
            set { SetValue(SaveEditCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveEditCommandProperty =
            DependencyProperty.Register("SaveEditCommand", typeof(ICommand), typeof(DisplayNote), new PropertyMetadata(null, SetSave));

        private static void SetSave(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNote control) return;
            control.SaveButton.IsEnabled = control.SaveEditCommand is not null;
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNote control) return;
            if (control.Note == null) return;

            control.DataContext = control.Note;


            DateTime SentAt = DateTime.Parse(control.Note.DateSent);
            control.SentDateTextBlock.Text = GetDateString(SentAt);
            control.SentAtTextBlock.Text = SentAt.ToString("HH:mm");

            control.EditedTextBoxOne.Visibility = control.Note.IsEdited && !control.Note.IsDeleted
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.EditedTextBoxTwo.Visibility = control.Note.IsEdited && !control.Note.IsDeleted
               ? Visibility.Visible
               : Visibility.Collapsed;

            control.JustifyMessage(right: control.Note.SentBy == App.CurrentUser.UserName, !control.Note.ShowHeader, control.Note.IsDeleted);

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

            control.spacer.Visibility = control.Note.SpaceUnder
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.SetOriginalMessageVis();
        }

        private void JustifyMessage(bool right = false, bool stacked = false, bool deleted = false)
        {
            ControlGrid.ColumnDefinitions[0].Width = right ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            ControlGrid.ColumnDefinitions[2].Width = !right ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
            MetadataStackPanel.HorizontalAlignment = right ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            int topLeft;
            int topRight;
            int bottomLeft;
            int bottomRight;

            if (right)
            {
                topLeft = 5;
                bottomLeft = 5;
                bottomRight = 0;
                topRight = stacked ? 0 : 5;
            }
            else
            {
                topLeft = stacked ? 0 : 5;
                bottomLeft = 0;
                bottomRight = 5;
                topRight = 5;
            }

            bg.CornerRadius = new(topLeft, topRight, bottomRight, bottomLeft);

            Brush brush = (Brush)Application.Current.Resources[right ? "Blue" : "Green"];
            Brush bgBrush = (Brush)Application.Current.Resources[right ? "BlueFaded" : "GreenFaded"];

            if (deleted)
            {
                brush = (Brush)Application.Current.Resources["Red"];
                bgBrush = (Brush)Application.Current.Resources["RedFaded"];
            }

            bg.Background = bgBrush;
            bg.BorderBrush = brush;
            SentByTextBlock.Foreground = brush;
            SentAtTextBlock.Foreground = brush;
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

            EditButton.Visibility = Visibility.Collapsed;

            CancelButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;

            EditBox.Focus();
            EditBox.Select(EditBox.Text.Length, 0);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            SaveEditCommand?.Execute(Note);

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void SetOriginalMessageVis()
        {
            OriginalMessage.Visibility = Visibility.Collapsed;

            if (App.CurrentUser.Role == UserRole.Administrator)
            {
                if (Note.IsEdited)
                {
                    OriginalMessage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Note.IsDeleted)
                {
                    MessageTextBlock.Text = "deleted message";
                    MessageTextBlock.FontStyle = FontStyles.Italic;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Text = Note.Message;
            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Note is null) return;
            if (Note.SentBy == App.CurrentUser.UserName && Note.ShowEdit && SaveEditCommand is not null && !Note.IsDeleted)
            {
                EditControls.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            EditControls.Visibility = Visibility.Hidden;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Text = Note.Message;
            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;

            DeleteCommand.Execute(Note);

            Brush brush = (Brush)Application.Current.Resources["Red"];
            Brush bgBrush = (Brush)Application.Current.Resources["RedFaded"];

            bg.Background = bgBrush;
            bg.BorderBrush = brush;
            SentByTextBlock.Foreground = brush;
            SentAtTextBlock.Foreground = brush;

            SetOriginalMessageVis();
        }
    }
}
