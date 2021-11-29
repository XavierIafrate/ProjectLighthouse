using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayNote : UserControl
    {
        public Delegate EditCommand
        {
            get { return (Delegate)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(Delegate), typeof(DisplayNote), new PropertyMetadata(null, SetValues));


        public Note Note
        {
            get => (Note)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        // Using a DependencyProperty as the backing store for Note.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(DisplayNote), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DisplayNote control)
            {
                if (control.Note != null)
                {
                    //control.Note.PropertyChanged += EditButton_Click();


                    control.MessageTextBlock.Text = control.Note.Message;

                    DateTime SentAt = DateTime.Parse(control.Note.DateSent);
                    control.SentAtTextBlock.Text = SentAt.ToString("dd-MMM HH:mm");
                    control.SentByTextBlock.Text = control.Note.SentBy;

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

                    //control.EditControls.Visibility = control.Note.ShowEdit
                    //    ? Visibility.Collapsed
                    //    : Visibility.Collapsed;
                }
            }
        }

        public DisplayNote()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Note.IsEdited = !Note.IsEdited; //toggle test
        }
    }
}
