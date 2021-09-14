using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class ComposeMessage : UserControl
    {
        public Note Note
        {
            get { return (Note)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Note.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register("Note", typeof(Note), typeof(ComposeMessage), new PropertyMetadata(null));

        public ComposeMessage()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (Note == null)
            {
                return;
            }
            Note.Message = Message.Text.Trim();
            //if (!string.IsNullOrWhiteSpace(Message.Text) && command != null && DocRef != null)
            //{
            //    Note newNote = new()
            //    {
            //        Message = Message.Text.Trim(),
            //        OriginalMessage = "",
            //        IsEdited = false,
            //        IsDeleted = false,
            //        SentBy = App.CurrentUser.UserName,
            //        DateSent = DateTime.Now.ToString("s"),
            //        DocumentReference = DocRef
            //    };

            //    DatabaseHelper.Insert(newNote);

            //    command.Execute(newNote);
            //}
        }
    }
}
