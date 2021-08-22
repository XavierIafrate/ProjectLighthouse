using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectLighthouse.Model;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayNote : UserControl
    {
        public Note Note
        {
            get { return (Note)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Note.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(DisplayNote), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNote control)
            {
                return;
            }

            if (control.Note == null)
            {
                return;
            }

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

            control.EditControls.Visibility = control.Note.ShowEdit
                ? Visibility.Collapsed
                : Visibility.Collapsed;
        }

        public DisplayNote()
        {
            InitializeComponent();
        }
    }
}
