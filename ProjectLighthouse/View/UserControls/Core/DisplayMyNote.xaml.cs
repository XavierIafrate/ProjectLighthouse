using DocumentFormat.OpenXml.Wordprocessing;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Commands.Orders;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMyNote : UserControl
    {
        public Note Note
        {
            get => (Note)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(DisplayMyNote), new PropertyMetadata(null, SetValues));


        public DeleteNoteCommand DeleteCommand
        {
            get { return (DeleteNoteCommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(DeleteNoteCommand), typeof(DisplayMyNote), new PropertyMetadata(null, SetDelete));

        private static void SetDelete(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMyNote control) return;
            control.DeleteButton.IsEnabled = control.DeleteCommand is not null;
        }


        public SaveNoteCommand SaveEditCommand
        {
            get { return (SaveNoteCommand)GetValue(SaveEditCommandProperty); }
            set { SetValue(SaveEditCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveEditCommandProperty =
            DependencyProperty.Register("SaveEditCommand", typeof(SaveNoteCommand), typeof(DisplayMyNote), new PropertyMetadata(null, SetSave));

        private static void SetSave(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMyNote control) return;
            control.SaveButton.IsEnabled = control.SaveEditCommand is not null;
        }


        private bool editing;

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMyNote control) return;
            if (control.Note == null) return;


            DateTime SentAt = DateTime.Parse(control.Note.DateSent);
            control.SentAtTextBlock.Text = SentAt.ToString("HH:mm");


            control.DevBadge.Visibility = control.Note.SentBy == "xav"
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

        public DisplayMyNote()
        {
            InitializeComponent();
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            editing = true;
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
            editing = false;

            MessageTextBlock.Visibility = Visibility.Visible;

            EditBox.Visibility = Visibility.Collapsed;
            EditBox.IsEnabled = false;

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(EditBox.Text))
            {
                return;
            }


            Note.Message = EditBox.Text.Trim();

            SaveEditCommand?.Execute(Note);
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
            
            if (Note.IsDeleted)
            {
                if(App.CurrentUser.Role != UserRole.Administrator)
                {
                    MessageTextBlock.Text = "deleted message";
                }
                MessageTextBlock.FontStyle = FontStyles.Italic;


                Brush brush = (Brush)Application.Current.Resources["Red"];
                Brush bgBrush = (Brush)Application.Current.Resources["RedFaded"];

                bg.Background = bgBrush;
                bg.BorderBrush = brush;
                SentByTextBlock.Foreground = brush;
                SentAtTextBlock.Foreground = brush;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            editing = false;

            MessageTextBlock.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;

            EditBox.Text = Note.Message;
            EditBox.IsEnabled = false;

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Note is null) return;
            if (Note.SentBy == App.CurrentUser.UserName && SaveEditCommand is not null && !Note.IsDeleted)
            {
                EditControls.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!editing)
            {
                EditControls.Visibility = Visibility.Hidden;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            editing = false;

            EditButton.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;

            DeleteCommand?.Execute(Note);
        }
    }
}
