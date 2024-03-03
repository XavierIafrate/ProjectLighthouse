using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

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



        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(DisplayListOfNotes), new PropertyMetadata(null));

        public ICommand SaveEditCommand
        {
            get { return (ICommand)GetValue(SaveEditCommandProperty); }
            set { SetValue(SaveEditCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveEditCommandProperty =
            DependencyProperty.Register("SaveEditCommand", typeof(ICommand), typeof(DisplayListOfNotes), new PropertyMetadata(null));

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

        private void Compute()
        {
            DisplayData = FormatListOfNotes(Notes, false);
        }

        public static List<object> FormatListOfNotes(List<Note> notes, bool edit = false)
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
        }
    }
}
