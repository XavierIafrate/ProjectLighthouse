using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayUserNote : UserControl
    {
        public Note Note
        {
            get => (Note)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(DisplayUserNote), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUserNote control) return;
            if (control.Note == null) return;

            DateTime sentAt = DateTime.Parse(control.Note.DateSent);
            control.SentAtTextBlock.Text = sentAt.ToString("HH:mm");

            control.DevBadge.Visibility = control.Note.SentBy == "xav"
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.OriginalMessage.Visibility = Visibility.Collapsed;

            if (App.CurrentUser.Role == UserRole.Administrator)
            {
                if (control.Note.IsEdited)
                {
                    control.OriginalMessage.Visibility = Visibility.Visible;
                }
            }

            if (control.Note.IsDeleted)
            {
                if (App.CurrentUser.Role != UserRole.Administrator)
                {
                    control.MessageTextBlock.Text = "deleted message";
                }
                control.MessageTextBlock.FontStyle = FontStyles.Italic;


                Brush brush = (Brush)Application.Current.Resources["Red"];
                Brush bgBrush = (Brush)Application.Current.Resources["RedFaded"];

                control.bg.Background = bgBrush;
                control.bg.BorderBrush = brush;
                control.SentByTextBlock.Foreground = brush;
                control.SentAtTextBlock.Foreground = brush;
                control.MessageTextBlock.Foreground = brush;
            }
            else if (control.Note.IsEdited)
            {
                control.EditedTextBox.Visibility = Visibility.Visible;
            }
        }

        public DisplayUserNote()
        {
            InitializeComponent();
        }
    }
}
