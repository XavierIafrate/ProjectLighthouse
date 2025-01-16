using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayListOfNotes : UserControl, INotifyPropertyChanged
    {
        public bool ShowingEditControls
        {
            get { return (bool)GetValue(ShowingEditControlsProperty); }
            set { SetValue(ShowingEditControlsProperty, value); }
        }

        public static readonly DependencyProperty ShowingEditControlsProperty =
            DependencyProperty.Register("ShowingEditControls", typeof(bool), typeof(DisplayListOfNotes), new PropertyMetadata(false, SetValues));

        public List<Note> Notes
        {
            get { return (List<Note>)GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(List<Note>), typeof(DisplayListOfNotes), new PropertyMetadata(null, SetValues));

        private List<object> displayData;
        public List<object> DisplayData
        {
            get { return displayData; }
            set { displayData = value; OnPropertyChanged(); }
        }

        public DeleteNoteCommand DeleteCommand
        {
            get { return (DeleteNoteCommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(DeleteNoteCommand), typeof(DisplayListOfNotes), new PropertyMetadata(null, SetDelete));

        private static void SetDelete(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(DisplayListOfNotes), new PropertyMetadata(null));

        public ICommand AddNoteCommand
        {
            get { return (ICommand)GetValue(AddNoteCommandProperty); }
            set { SetValue(AddNoteCommandProperty, value); }
        }

        public static readonly DependencyProperty AddNoteCommandProperty =
            DependencyProperty.Register("AddNoteCommand", typeof(ICommand), typeof(DisplayListOfNotes), new PropertyMetadata(null));

        private bool enterToSend = true;

        public bool EnterToSend
        {
            get { return enterToSend; }
            set { enterToSend = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayListOfNotes control) return;
            control.Compute();
        }

        private Note newNote;

        public Note NewNote
        {
            get { return newNote; }
            set { newNote = value; OnPropertyChanged(); }
        }



        private void Compute()
        {
            DisplayData = FormatListOfNotes(Notes);
        }

        public static List<object> FormatListOfNotes(List<Note> notes)
        {
            if (notes == null) return null;
            if (notes.Count == 0) return new();

            List<object> results = new();

            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;
            List<User> users = DatabaseHelper.Read<User>();

            for (int i = 0; i < notes.Count; i++)
            {
                bool dateHeaderAdded = false;
                Note note = notes[i];
                DateTime noteSent = DateTime.Parse(note.DateSent);
                if (noteSent.Date != lastTimeStamp.Date)
                {
                    results.Add(noteSent);
                    dateHeaderAdded = true;
                }

                if (i == 0)
                {
                    note.ShowHeader = true;
                }
                else
                {
                    if (notes[i - 1].SentBy != note.SentBy || dateHeaderAdded)
                    {
                        note.ShowHeader = true;
                    }
                }


                if (i < notes.Count - 1)
                {
                    Note nextNote = notes[i + 1];
                    notes[i].SpaceUnder = note.SentBy != nextNote.SentBy || DateTime.Parse(note.DateSent).AddHours(6) < DateTime.Parse(nextNote.DateSent);
                }

                lastTimeStamp = DateTime.Parse(note.DateSent);
                name = note.SentBy;
                note.UserDetails = users.Find(x => x.UserName == name);

                if (i == notes.Count - 1)
                {
                    note.SpaceUnder = true;
                }
                results.Add(note);
            }

            return results;
        }

        public DisplayListOfNotes()
        {
            InitializeComponent();
            NewNote = new()
            {
                SentBy = App.CurrentUser.UserName,
            };
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            NewNote.ValidateAll();
            if (NewNote.HasErrors)
            {
                return;
            }

            NewNote.DateSent = DateTime.Now.ToString("s");
            AddNoteCommand?.Execute(NewNote);
            NewNote = new()
            {
                SentBy = App.CurrentUser.UserName,
            };
        }

        private void MessageComposer_KeyDown(object sender, KeyEventArgs e)
        {
            NewNote.DateSent = DateTime.Now.ToString("s");
            if (e.Key == Key.Enter && EnterToSend)
            {
                NewNote.ValidateAll();
                if (NewNote.HasErrors)
                {
                    return;
                }

                AddNoteCommand?.Execute(NewNote);
            }
            else
            {
                return;
            }

            NewNote = new()
            {
                SentBy = App.CurrentUser.UserName,
            };
        }
    }
}
