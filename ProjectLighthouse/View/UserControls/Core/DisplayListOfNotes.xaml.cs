﻿using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayListOfNotes : UserControl
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


        public List<Note> displayNotes { get; set; }


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

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayListOfNotes control) return;

            control.displayNotes = FormatListOfNotes(control.Notes, control.ShowingEditControls);
            control.NotesList.ItemsSource = control.displayNotes;

            control.noneText.Visibility = control.NotesList.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static List<Note> FormatListOfNotes(List<Note> notes, bool edit = false)
        {
            if (notes == null) return null;
            if (notes.Count == 0) return notes;

            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;
            List<User> users = DatabaseHelper.Read<User>();

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].ShowEdit = notes[i].SentBy == App.CurrentUser.UserName;
                notes[i].ShowHeader = notes[i].SentBy != name
                    || DateTime.Parse(notes[i].DateSent) > lastTimeStamp.AddHours(6);

                notes[i].ShowDateHeader = DateTime.Parse(notes[i].DateSent).Date != lastTimeStamp.Date;

                if (i < notes.Count - 1)
                {
                    notes[i].SpaceUnder = notes[i].SentBy != notes[i + 1].SentBy || DateTime.Parse(notes[i].DateSent).AddHours(6) < DateTime.Parse(notes[i + 1].DateSent);
                }
                lastTimeStamp = DateTime.Parse(notes[i].DateSent);
                name = notes[i].SentBy;
                notes[i].UserDetails = users.Find(x => x.UserName == notes[i].SentBy);
                notes[i].ShowEdit = edit;
            }

            return notes;
        }

        public DisplayListOfNotes()
        {
            InitializeComponent();
        }
    }
}
