using ProjectLighthouse.Model;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class EditLMOWindow : Window
    {
        public LatheManufactureOrder order;
        public List<LatheManufactureOrderItem> items;
        public List<Lot> lots;
        public bool SaveExit { get; set; }
        public List<Note> Notes;

        public EditLMOWindow(LatheManufactureOrder o, List<LatheManufactureOrderItem> i, List<Lot> l, List<Note> n)
        {
            InitializeComponent();

            SaveExit = false;
            items = new(i);
            lots = new(l);
            Notes = n;

            FormatNoteDisplay();

            order = (LatheManufactureOrder)o.Clone(); // break the reference

            PopulateControls();
        }

        private void FormatNoteDisplay()
        {
            string name = "";
            foreach (Note note in Notes)
            {
                DateTime DateOfNote = DateTime.Parse(note.DateSent);
                note.ShowEdit = DateOfNote.AddDays(14) > DateTime.Now && note.SentBy == App.CurrentUser.UserName;

                note.ShowHeader = name != note.SentBy;
                name = note.SentBy;
            }
        }

        private void PopulateControls()
        {
            ItemsListBox.ItemsSource = items;
            NotesDisplay.ItemsSource = Notes;

            nameText.Text = order.Name;
            PORef.Text = order.POReference;

            program.IsChecked = order.HasProgram;
            ready.IsChecked = order.IsReady;

            ready.IsEnabled = order.HasProgram;


            switch (order.Status)
            {
                case "Running":
                    ready.IsEnabled = false;
                    program.IsEnabled = false;
                    running_radio.IsChecked = true;
                    break;
                case "Complete":
                    ready.IsEnabled = false;
                    program.IsEnabled = false;
                    complete_radio.IsChecked = true;
                    break;
                case "Problem":
                    running_radio.IsEnabled = false;
                    complete_radio.IsEnabled = false;
                    not_started_radio.IsChecked = true;
                    break;
                default:
                    not_started_radio.IsChecked = true;
                    break;
            }

            PORef.IsEnabled = App.CurrentUser.CanEditLMOs || App.CurrentUser.UserRole == "admin";
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            AssignValues();

            order.ModifiedBy = App.CurrentUser.GetFullName();
            order.ModifiedAt = DateTime.Now;

            DatabaseHelper.Update(order);
            SaveExit = true;
            Close();
        }

        private void AssignValues()
        {
            order.POReference = PORef.Text;
            order.HasProgram = (bool)program.IsChecked;
            order.IsReady = (bool)ready.IsChecked;

            if ((bool)not_started_radio.IsChecked)
            {
                order.Status = order.IsReady ? "Ready" : "Problem";
            }
            else
            {
                order.Status = (bool)running_radio.IsChecked ? "Running" : "Complete";
            }



            order.IsComplete = order.Status == "Complete";

            calculateTime();
            CalculateBarRequirements();
        }

        private void program_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked ?? false)
            {
                ready.IsEnabled = true;
            }
            else
            {
                ready.IsEnabled = false;
                ready.IsChecked = false;
                running_radio.IsEnabled = false;
                complete_radio.IsEnabled = false;
            }
        }

        private void DisplayLMOItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisplayLMOItems control = sender as DisplayLMOItems;
            EditLMOItemWindow editWindow = new(control.LatheManufactureOrderItem, lots);
            Hide();
            editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                SaveExit = true;
                lots = new List<Lot>(editWindow.Lots);
                RefreshItems();
            }
            ShowDialog();
        }

        public void RefreshItems()
        {
            items = new List<LatheManufactureOrderItem>();
            items = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(n => n.AssignedMO == order.Name).
                OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName).ToList();
            ItemsListBox.ItemsSource = new List<LatheManufactureOrderItem>(items);
        }


        #region Helpers
        private void calculateTime()
        {
            int estimatedTimeSeconds = 0;
            foreach (LatheManufactureOrderItem item in items)
                estimatedTimeSeconds += item.CycleTime * item.TargetQuantity;

            order.TimeToComplete = estimatedTimeSeconds;
        }

        private void CalculateBarRequirements()
        {
            List<BarStock> barStock = DatabaseHelper.Read<BarStock>();
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();
            double totalLengthRequired = 0;

            foreach (LatheManufactureOrderItem item in items)
            {
                TurnedProduct _p = products.Where(n => n.ProductName == item.ProductName).Single();
                totalLengthRequired += (_p.MajorLength + 2) * item.TargetQuantity;
            }

            order.NumberOfBars = Math.Ceiling(totalLengthRequired / 2700);
        }

        private void ready_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked ?? false)
            {
                order.Status = "Ready";
                running_radio.IsEnabled = true;
                complete_radio.IsEnabled = true;
            }
            else
            {
                order.Status = "Problem";
                running_radio.IsEnabled = false;
                complete_radio.IsEnabled = false;
            }
        }

        private void CancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel this order?", "Cancel Order", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Not Implemented!!");
            }
        }

        private void not_started_radio_Checked(object sender, RoutedEventArgs e)
        {
            ready.IsEnabled = true;
            program.IsEnabled = true;
        }

        private void running_radio_Checked(object sender, RoutedEventArgs e)
        {
            ready.IsEnabled = false;
            program.IsEnabled = false;
        }

        private void complete_radio_Checked(object sender, RoutedEventArgs e)
        {
            ready.IsEnabled = false;
            program.IsEnabled = false;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Timestamp and Initials keyb shortcut
        private void notes_KeyDown(object sender, KeyEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            if (e.Key == Key.F2)
            {
                TextRange range = new(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                range.Text = $"({DateTime.Now:dd/MM/yy HH:mm} - {App.CurrentUser.FirstName[0].ToString().ToUpper()}{App.CurrentUser.LastName[0].ToString().ToUpper()}) ";
                Debug.WriteLine(App.CurrentUser.FirstName);
                Debug.WriteLine(App.CurrentUser.FirstName[0].ToString());
                TextPointer caretPos = richTextBox.CaretPosition;
                richTextBox.CaretPosition = caretPos.DocumentEnd;
            }
        }
        #endregion

        private void PORef_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            poRefGhostText.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text))
            {
                AddNewNote(Message.Text.Trim());
            }
        }

        private void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComposeGhost.Visibility = string.IsNullOrEmpty(Message.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public void AddNewNote(string note)
        {
            Note newNote = new()
            {
                Message = note,
                OriginalMessage = note,
                IsEdited = false,
                IsDeleted = false,
                SentBy = App.CurrentUser.UserName,
                DateSent = DateTime.Now.ToString("s"),
                DateEdited = DateTime.MinValue.ToString("s"),
                DocumentReference = order.Name
            };

            DatabaseHelper.Insert(newNote);
            Notes.Add(newNote);
            NotesDisplay.ItemsSource = new List<Note>();
            NotesDisplay.ItemsSource = Notes;
            Message.Text = "";

            FormatNoteDisplay();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text) && e.Key == Key.Enter)
            {
                AddNewNote(Message.Text.Trim());
            }
        }
    }
}
