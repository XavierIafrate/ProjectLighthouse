using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

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
            if (d is DisplayNote control)
            {
                if (control.Note != null)
                {
                    control.DataContext = control.Note;
                    control.MessageTextBlock.Text = control.Note.Message;

                    DateTime SentAt = DateTime.Parse(control.Note.DateSent);
                    control.SentAtTextBlock.Text = SentAt.ToString("dd-MMM HH:mm");

                    control.SentByTextBlock.Text = control.Note.SentBy;

                    DateTime EditedAt = DateTime.Parse(control.Note.DateEdited);
                    control.EditedAtTextBlock.Text = EditedAt.ToString("dd-MMM HH:mm");
                    control.EditedAtTextBlock.Visibility = EditedAt == DateTime.MinValue
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    if (control.Note.SentBy == App.CurrentUser.UserName)
                    {
                        control.ControlGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                        control.ControlGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
                        control.MessageTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                        control.MessageTextBlock.TextAlignment = TextAlignment.Right;
                        control.MetadataStackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                        control.TextBlockContainer.HorizontalAlignment = HorizontalAlignment.Right;
                    }

                    control.DevBadge.Visibility = control.Note.SentBy == "xav"
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    control.MetadataStackPanel.Visibility = control.Note.ShowHeader
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    control.ControlGrid.Margin = control.Note.ShowHeader
                        ? new Thickness(5, 5, 5, 5)
                        : new Thickness(5, 0, 5, 5);

                    control.EditControls.Visibility = control.Note.ShowEdit && App.CurrentUser.UserName == control.Note.SentBy
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    control.spacer.Visibility = control.Note.ShowSpacerUnder
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    control.SetOriginalMessageVis();
                }
            }
        }

        public DisplayNote()
        {
            InitializeComponent();
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

            if (MessageTextBlock.Text != Note.Message)
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
            OriginalMessage.Visibility = Note.Message != Note.OriginalMessage
                && App.CurrentUser.UserRole == "admin"
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
