﻿using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        #region Variables

        public List<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public List<LatheManufactureOrder> FilteredOrders { get; set; }
        public List<LatheManufactureOrderItem> LMOItems { get; set; }
        public List<LatheManufactureOrderItem> FilteredLMOItems { get; set; }
        public List<Note> Notes { get; set; }
        public List<Note> FilteredNotes { get; set; }

        public List<TurnedProduct> Products { get; set; }
        public List<TurnedProduct> SelectedProducts { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public ProductGroup SelectedProductGroup { get; set; }
        public List<MachineStatistics> MachineStatistics { get; set; }
        public List<Lot> Lots { get; set; }

        private LatheManufactureOrder selectedLatheManufactureOrder;

        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set
            {
                selectedLatheManufactureOrder = value;
                LoadOrderCard();
                OnPropertyChanged("SelectedLatheManufactureOrder");
            }
        }

        private MachineStatistics displayStats; // Stats for the machine listed on the order currently selected

        public MachineStatistics DisplayStats
        {
            get { return displayStats; }
            set
            {
                displayStats = value;
                OnPropertyChanged("DisplayStats");
            }
        }

        private string runInfoText;

        public string RunInfoText
        {
            get { return runInfoText; }
            set
            {
                runInfoText = value;
                OnPropertyChanged("RunInfoText");
            }
        }

        private string selectedFilter;

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterOrders(value);
                if (FilteredOrders.Count > 0)
                {
                    SelectedLatheManufactureOrder = FilteredOrders.First();
                    CardVis = Visibility.Visible;
                }
                else
                {
                    CardVis = Visibility.Hidden;
                }
            }
        }

        private string searchTerm;

        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value.ToUpper();
                OnPropertyChanged("SearchTerm");
                Search();
            }
        }

        #region Visibility variables

        private Visibility liveInfoVis;

        public Visibility LiveInfoVis
        {
            get { return liveInfoVis; }
            set
            {
                liveInfoVis = value;
                OnPropertyChanged("LiveInfoVis");
            }
        }

        private Visibility cleaningVis;

        public Visibility CleaningVis
        {
            get { return cleaningVis; }
            set
            {
                cleaningVis = value;
                OnPropertyChanged("CleaningVis");
            }
        }


        private Visibility cardVis;

        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged("CardVis");
                if (value == Visibility.Visible)
                {
                    NothingVis = Visibility.Hidden;
                    return;
                }
                NothingVis = Visibility.Visible;
            }
        }

        private Visibility nothingVis;

        public Visibility NothingVis
        {
            get { return nothingVis; }
            set
            {
                nothingVis = value;
                OnPropertyChanged("NothingVis");
            }
        }

        private Visibility modifiedVis;

        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set
            {
                modifiedVis = value;
                OnPropertyChanged("ModifiedVis");
            }
        }

        #endregion Visibility variables

        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public event EventHandler SelectedLatheManufactureOrderChanged;

        #endregion Variables

        public OrderViewModel()
        {
            Debug.WriteLine("Init: OrderViewModel");
            Notes = new();
            FilteredNotes = new();
            LatheManufactureOrders = new();
            FilteredOrders = new();
            MachineStatistics = new();
            LMOItems = new();
            FilteredLMOItems = new();
            SelectedLatheManufactureOrder = new();
            DisplayStats = new();
            ProductGroups = new();
            SelectedProductGroup = new();
            SelectedProducts = new();

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            Products = DatabaseHelper.Read<TurnedProduct>();

            GetLatheManufactureOrders();
            FilterOrders("All Active");
            GetLatheManufactureOrderItems();
            GetLatestStats();
        }

        #region MachineStats Display

        private void GetLatestStats()
        {
            MachineStatistics = null;
            MachineStatistics = MachineStatsHelper.GetStats() ?? new();
            //MachineStatistics ??= new List<MachineStatistics>();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();
            if (MachineStatistics.Count == 0)
                return;

            string latheName = lathes.Where(n => n.Id == SelectedLatheManufactureOrder.AllocatedMachine).FirstOrDefault().Id;

            DisplayStats = MachineStatistics.Where(n => n.MachineID == latheName).FirstOrDefault();

            if (DisplayStats == null)
            {
                LiveInfoVis = Visibility.Collapsed;
                return;
            }
            if (DisplayStats.DataTime.AddHours(1) < DateTime.Now)
                LiveInfoVis = Visibility.Collapsed;
        }

        #endregion MachineStats Display

        #region Loading

        private void GetLatheManufactureOrders()
        {
            LatheManufactureOrders.Clear();
            LatheManufactureOrders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
        }

        private void FilterOrders(string filter)
        {
            switch (filter)
            {
                case "All Active":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.State < OrderState.Complete || n.ModifiedAt.AddDays(1) > DateTime.Now));
                    break;

                case "Not Ready":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.State == OrderState.Problem));
                    break;

                case "Ready":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.State == OrderState.Ready || n.State == OrderState.Prepared));
                    break;

                case "Complete":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.State > OrderState.Running).OrderByDescending(n => n.CreatedAt));
                    break;

                case "Search":
                    break;
            }

            if (FilteredOrders.Count > 0)
            {
                SelectedLatheManufactureOrder = FilteredOrders.First();
            }

            OnPropertyChanged("FilteredOrders");
        }

        private void GetLatheManufactureOrderItems()
        {
            LMOItems.Clear();
            LMOItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            Notes.Clear();
            Notes = DatabaseHelper.Read<Note>().ToList();

        }

        private void LoadLMOItems()
        {
            if (SelectedLatheManufactureOrder == null)
            {
                return;
            }

            string selectedMO = SelectedLatheManufactureOrder.Name;
            FilteredLMOItems.Clear();

            IEnumerable<LatheManufactureOrderItem> associatedItems = LMOItems.Where(i => i.AssignedMO == selectedMO);

            FilteredLMOItems = new(associatedItems.OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName));
            if (App.CurrentUser.UserRole == "admin")
            {
                FilteredNotes = Notes.Where(n => n.DocumentReference == selectedMO).OrderBy(x => x.DateSent).ToList();
            }
            else
            {
                FilteredNotes = Notes.Where(n => n.DocumentReference == selectedMO && !n.IsDeleted).OrderBy(x => x.DateSent).ToList();
            }

            string name = "";

            foreach (Note note in FilteredNotes) // Tidy headers
            {
                note.ShowEdit = false;
                note.ShowHeader = note.SentBy != name;
                name = note.SentBy;
            }

            OnPropertyChanged("FilteredLMOItems");
            OnPropertyChanged("FilteredNotes");
        }

        #endregion Loading

        public void Search()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilterOrders(SelectedFilter);
                return;
            }

            List<LatheManufactureOrder> Results = new();
            List<string> FoundOrders = new();

            foreach (LatheManufactureOrder order in LatheManufactureOrders)
            {
                if (order.Name.Contains(SearchTerm))
                {
                    Results.Add(order);
                    FoundOrders.Add(order.Name);
                    continue;
                }

                if (order.POReference != null)
                {
                    if (order.POReference.Contains(SearchTerm) && order.POReference != "N/A")
                    {
                        Results.Add(order);
                        FoundOrders.Add(order.Name);
                        continue;
                    }
                }
            }

            List<string> FoundOrdersByItem = new();
            foreach (LatheManufactureOrderItem item in LMOItems)
            {
                if (FoundOrders.Contains(item.AssignedMO))
                {
                    continue;
                }

                if (item.ProductName.Contains(SearchTerm))
                {
                    FoundOrdersByItem.Add(item.AssignedMO);
                    continue;
                }
            }

            Results.AddRange(LatheManufactureOrders.Where(x => FoundOrdersByItem.Contains(x.Name)));

            FilteredOrders = Results;
            FilterOrders("Search");
        }

        private void LoadOrderCard()
        {
            if (SelectedLatheManufactureOrder == null)
            {
                CardVis = Visibility.Hidden;
                return;
            }
            else
            {
                CardVis = Visibility.Visible;
            }

            LoadLMOItems();

            ModifiedVis = string.IsNullOrEmpty(SelectedLatheManufactureOrder.ModifiedBy)
                ? Visibility.Collapsed
                : Visibility.Visible;
            LiveInfoVis = SelectedLatheManufactureOrder.State == OrderState.Running && MachineStatistics.Count != 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            CleaningVis = SelectedLatheManufactureOrder.ItemNeedsCleaning
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (LiveInfoVis == Visibility.Visible)
                GetLatestStats();

            Lots = DatabaseHelper.Read<Lot>().ToList();

            RunInfoText = !string.IsNullOrEmpty(SelectedLatheManufactureOrder.AllocatedMachine)
                ? $"Assigned to {selectedLatheManufactureOrder.AllocatedMachine}, starting {SelectedLatheManufactureOrder.StartDate:dddd, MMMM d}{GetDaySuffix(SelectedLatheManufactureOrder.StartDate.Day)}"
                : "Not scheduled";

            LoadProductInfoCard();
        }

        private void LoadProductInfoCard()
        {
            SelectedProducts = new();
            foreach (LatheManufactureOrderItem item in FilteredLMOItems)
            {
                List<TurnedProduct> tmp = Products.Where(n => n.ProductName == item.ProductName).ToList();
                if (tmp.Count > 0)
                {
                    SelectedProducts.Add(tmp.First());
                }
            }

            if (SelectedProducts.Count != 0)
            {
                TurnedProduct tmp = SelectedProducts.First();
                List<ProductGroup> matches = ProductGroups.Where(x => x.GroupID == tmp.ProductName.Substring(0, 5) && x.MaterialCode == tmp.Material).ToList();
                SelectedProductGroup = matches.Count != 0 ? matches.First() : new();
            }

            OnPropertyChanged("SelectedProductGroup");
            OnPropertyChanged("SelectedProduct");
        }

        public void PrintSelectedOrder()
        {
            PDFHelper.PrintOrder(SelectedLatheManufactureOrder, FilteredLMOItems, FilteredNotes);
        }

        public void EditLMO()
        {
            EditLMOWindow editWindow = new(SelectedLatheManufactureOrder, FilteredLMOItems, Lots.Where(n => n.Order == SelectedLatheManufactureOrder.Name).ToList(), FilteredNotes);
            editWindow.Owner = Application.Current.MainWindow;
            _ = editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                GetLatheManufactureOrders();
                FilterOrders(SelectedFilter); // update list on screen

                SelectedLatheManufactureOrder = LatheManufactureOrders.Where(o => o.Name == editWindow.order.Name).FirstOrDefault();
            }

            editWindow = null;

            GetLatheManufactureOrderItems();
            LoadLMOItems();
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }

        //private void DebugWriteFile()
        //{
        //    CSVHelper.WriteListToCSV(LatheManufactureOrders, "test");
        //}
    }
}