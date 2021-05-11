using ProjectLighthouse.Model;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectLighthouse.View
{
    public partial class EditLMOWindow : Window
    {
        private LatheManufactureOrder order;
        private ObservableCollection<LatheManufactureOrderItem> items;

        public EditLMOWindow(LatheManufactureOrder o)
        {
            InitializeComponent();
            order = o;
            items = new ObservableCollection<LatheManufactureOrderItem>();
            PopulateControls();
        }

        private void PopulateControls()
        {
            var itemsList = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(n => n.AssignedMO == order.Name).ToList();
            items.Clear();
            foreach (var product in itemsList)
            {
                items.Add(product);
            }

            ItemsListBox.ItemsSource = items;

            nameText.Text = order.Name;
            PORef.Text = order.POReference;
            notes.Document.Blocks.Clear();
            notes.AppendText(order.Notes);
            notes.Document.LineHeight = 2;
            urgent.IsChecked = order.IsUrgent;
            program.IsChecked = order.HasProgram;
            ready.IsChecked = order.IsReady;

            if (!order.HasProgram)
            {
                ready.IsEnabled = false;
            }

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

            if (!App.currentUser.CanEditLMOs)
            {
                PORef.IsEnabled = false;
                setters.IsEnabled = false;
            }

            cancelOrderButton.Visibility = (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin") ? 
                Visibility.Visible : Visibility.Collapsed;
            
            var users = DatabaseHelper.Read<User>().Where(n => n.UserRole == "Production").ToList();
            List<string> setterUsers = new List<string>();

            foreach (var user in users)
            {
                setterUsers.Add(user.GetFullName());
            }
            setters.ItemsSource = setterUsers.ToList();
            setters.Text = order.AllocatedSetter;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(notes.Document.ContentStart, notes.Document.ContentEnd);

            if (textRange.Text.Length > 2)
                order.Notes = textRange.Text.Substring(0, textRange.Text.Length - 2);
           
            order.POReference = PORef.Text;
            order.AllocatedSetter = setters.Text;
            //order.IsUrgent = urgent.IsChecked ?? false;
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

            order.ModifiedBy = App.currentUser.GetFullName();
            order.ModifiedAt = DateTime.Now;

            DatabaseHelper.Update(order);

            this.Close();
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
            LatheManufactureOrderItem item = control.LatheManufactureOrderItem;
            EditLMOItemWindow editWindow = new EditLMOItemWindow(item);
            editWindow.ShowDialog();
            PopulateControls();
        }

        private void calculateTime()
        {
            int estimatedTimeSeconds = 0;
            foreach (var item in items)
            {
                estimatedTimeSeconds += item.CycleTime * item.TargetQuantity;
            }

            order.TimeToComplete = estimatedTimeSeconds;
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

        private void notes_KeyDown(object sender, KeyEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            if (e.Key == Key.F2)
            {
                TextRange range = new TextRange(richTextBox.Document.ContentEnd, richTextBox.Document.ContentEnd);
                range.Text = string.Format("({0:dd/MM/yy HH:mm} - {1}{2}) ", 
                    DateTime.Now, 
                    App.currentUser.FirstName[0].ToString().ToUpper(), 
                    App.currentUser.LastName[0].ToString().ToUpper());
                Debug.WriteLine(App.currentUser.FirstName);
                Debug.WriteLine(App.currentUser.FirstName[0].ToString());
                TextPointer caretPos = richTextBox.CaretPosition;
                richTextBox.CaretPosition = caretPos.DocumentEnd;
            }
        }
    }
}
