using ProjectLighthouse.Model;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class EditLMOWindow : Window, INotifyPropertyChanged
    {
        #region Variables

        private LatheManufactureOrder _order;
        public LatheManufactureOrder order
        {
            get { return _order; }
            set
            {
                _order = value;
                SetCheckboxEnabling();
                OnPropertyChanged();
            }
        }

        public LatheManufactureOrder savedOrder;

        public List<LatheManufactureOrderItem> items { get; set; }
        public List<Lot> lots;
        public bool SaveExit { get; set; }
        public List<Note> Notes { get; set; }   

        public event PropertyChangedEventHandler PropertyChanged;

        public List<BarStock> BarStock { get; set; }

        #endregion

        public EditLMOWindow(LatheManufactureOrder o, List<LatheManufactureOrderItem> i, List<Lot> l, List<Note> n, List<BarStock> b)
        {
            InitializeComponent();

            SaveExit = false;

            items = new();
            foreach (LatheManufactureOrderItem item in i)
            {
                item.RequestToEdit += EditItem;
                items.Add(item);
                item.ShowEdit = App.CurrentUser.CanUpdateLMOs;
            }

            lots = new(l);
            Notes = n;
            BarStock = b;
            OnPropertyChanged(nameof(BarStock));

            FormatNoteDisplay();

            savedOrder = o;
            order = (LatheManufactureOrder)o.Clone(); // break the reference

            DataContext = this;

            UpdateControls();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void EditItem(LatheManufactureOrderItem item)
        {
            EditLMOItemWindow editWindow = new(item, lots, order.AllocatedMachine ?? "");
            Hide();
            editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                SaveExit = true;
                lots = new List<Lot>(editWindow.Lots);
                RefreshItems();
            }
            ShowDialog();
            items = new(items);
            OnPropertyChanged(nameof(items));
        }

        private void FormatNoteDisplay()
        {
            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;

            for (int i = 0; i < Notes.Count; i++)
            {
                Notes[i].ShowEdit = Notes[i].SentBy == App.CurrentUser.UserName;
                Notes[i].ShowHeader = Notes[i].SentBy != name
                    || DateTime.Parse(Notes[i].DateSent) > lastTimeStamp.AddHours(6);
                if (i < Notes.Count - 1)
                {
                    Notes[i].ShowSpacerUnder = DateTime.Parse(Notes[i + 1].DateSent) > DateTime.Parse(Notes[i].DateSent).AddHours(6);
                }
                lastTimeStamp = DateTime.Parse(Notes[i].DateSent);
                name = Notes[i].SentBy;
            }
        }

        private void UpdateControls()
        {
            //ItemsListBox.ItemsSource = items;
            //NotesDisplay.ItemsSource = Notes;
            NotesScroller.ScrollToBottom();

            PORef.IsEnabled = App.CurrentUser.CanUpdateLMOs;
            BarStockComboBox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && !order.BarIsVerified;

            SetCheckboxEnabling();
        }

        private void SetCheckboxEnabling()
        {
            bool tier1;
            bool tier2;
            bool tier3;
            bool tier4;

            switch (order.State)
            {
                case >= OrderState.Complete:
                    tier1 = false;
                    tier2 = false;
                    tier3 = false;
                    tier4 = true;
                    break;
                case OrderState.Running:
                    tier1 = false;
                    tier2 = false;
                    tier3 = true;
                    tier4 = true;
                    break;
                case OrderState.Prepared:
                    tier1 = false;
                    tier2 = true;
                    tier3 = order.StartDate.Date <= DateTime.Today;
                    tier4 = false;
                    break;
                case OrderState.Problem or OrderState.Ready:
                    tier1 = true;
                    tier2 = order.BarIsVerified;
                    tier3 = false;
                    tier4 = false;
                    break;
                default:
                    tier1 = false;
                    tier2 = false;
                    tier3 = false;
                    tier4 = false;
                    break;
            }

            if (!App.CurrentUser.CanUpdateLMOs)
            {
                tier1 = false;
                tier2 = false;
                tier3 = false;
                tier4 = false;
            }

            Tooling_Checkbox.IsEnabled = tier1;
            Program_Checkbox.IsEnabled = tier1;
            BarVerified_Checkbox.IsEnabled = tier1;

            BarAllocated_Checkbox.IsEnabled = tier2;

            Running_Checkbox.IsEnabled = tier3;

            Complete_Checkbox.IsEnabled = tier4 && !order.IsCancelled;
            Cancelled_Checkbox.IsEnabled = App.CurrentUser.UserRole is "Scheduling" or "admin";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckIfOrderIsClosed()
        {
            if (order.State > OrderState.Running)
            {
                var itemsWithBadCycleTimes = items.Where(i => i.CycleTime == 0 && i.QuantityMade > 0).ToList();
                var unresolvedLots = lots.Where(l => l.Quantity != 0 && !l.IsDelivered && !l.IsReject).ToList();

                order.IsClosed = itemsWithBadCycleTimes.Count == 0 // ensure cycle time is updated
                    && unresolvedLots.Count == 0; // ensure lots are fully processed
            }
            else
            {
                order.IsClosed = false;
            }
        }

        private void DisplayLMOItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisplayLMOItems control = sender as DisplayLMOItems;
            if (App.CurrentUser.CanUpdateLMOs)
            {
                EditItem(control.LatheManufactureOrderItem);
            }
        }

        public void RefreshItems()
        {
            items = new();
            items = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(n => n.AssignedMO == order.Name)
                .OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName)
                .ToList();

            foreach (LatheManufactureOrderItem item in items)
            {
                item.ShowEdit = true;
                item.RequestToEdit += EditItem;
            }

            //ItemsListBox.ItemsSource = new List<LatheManufactureOrderItem>(items);
        }

        #region Helpers
        private void CalculateTime()
        {
            int estimatedTimeSeconds = 0;
            int cycleTime = 0;
            foreach (LatheManufactureOrderItem item in items.OrderByDescending(x => x.CycleTime))
            {
                if (item.CycleTime == 0 && cycleTime > 0) // uses better cycle time if available
                {
                    estimatedTimeSeconds += item.GetCycleTime() * item.TargetQuantity;
                }
                else
                {
                    estimatedTimeSeconds += item.GetCycleTime() * item.TargetQuantity;
                }
            }

            order.TimeToComplete = estimatedTimeSeconds;
        }

        private void CalculateBarRequirements()
        {
            List<BarStock> barStock = DatabaseHelper.Read<BarStock>();
            double totalLengthRequired = 0;
            BarStock bar = barStock.First(b=>b.Id == order.BarID);
            double partOff = 2;

            if (!string.IsNullOrEmpty(order.AllocatedMachine))
            {
                List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
                Lathe runningOnLathe = lathes.First(l => l.Id == order.AllocatedMachine);
                partOff = runningOnLathe.PartOff;
            }

            foreach (LatheManufactureOrderItem item in items)
            {
                totalLengthRequired += (item.MajorLength + partOff) * item.TargetQuantity * 1.02;
            }

            order.NumberOfBars = Math.Ceiling(totalLengthRequired / 2700);
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

        public void AddNewNote(string note) // Factor in edits
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
            SaveExit = true;

            FormatNoteDisplay();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text) && e.Key == Key.Enter)
            {
                AddNewNote(Message.Text.Trim());
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckboxEnabling();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CalculateTime();
            CalculateBarRequirements();
            CheckIfOrderIsClosed(); // running to closed is broken

            if (savedOrder.IsUpdated(order))
            {
                SaveExit = true;
            }

            if (SaveExit)
            {
                order.ModifiedBy = App.CurrentUser.GetFullName();
                order.ModifiedAt = DateTime.Now;

                order.Status = order.State.ToString();

                if (savedOrder.State < OrderState.Complete && order.State >= OrderState.Complete)
                {
                    order.CompletedAt = order.ModifiedAt;
                }

                _ = DatabaseHelper.Update(order);
            }
        }
    }
}
