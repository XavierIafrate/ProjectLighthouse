using DocumentFormat.OpenXml.Spreadsheet;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View.UserControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            DateTime SentAt = DateTime.Parse(control.Note.DateSent);
            control.SentAtTextBlock.Text = SentAt.ToString("HH:mm");

            control.DevBadge.Visibility =  control.Note.SentBy == "xav"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public DisplayUserNote()
        {
            InitializeComponent();
        }
    }
}
